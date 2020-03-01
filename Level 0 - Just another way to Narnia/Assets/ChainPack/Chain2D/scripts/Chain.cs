using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


public enum ChainMode
{
    None,
    Queue,
    Random,
}

[ExecuteInEditMode]
public class Chain : MonoBehaviour
{
    //public bool reset = false;
    public GameObject A;
    public GameObject B;

    // не понятно, почему если приватное поле, то теряется ссылка, после play
    // РАБОТАЕТ добавил аттрибут SerializeField
    [SerializeField]
    GameObject _tempA;

    [SerializeField]
    GameObject _tempB;

    [SerializeField]
    float anchorOffset = 0;

    [SerializeField]
    float anchorOffsetDummy = 0;//изменять в эдиторе будем это свойство, а anchorOffset будет брать с него значение в уменьшеном варианте, так как есть вероятность резкого возрастания количества звеньев при уменьшении оригинального anchorOffset.

    //[Header("Coil")]
    //[Range(-0.05f, 0.05f)]

    /// <summary>
    /// Speed of coil.
    /// </summary>
    public float coilASpeed = 0f;
    //[Range(-0.05f, 0.05f)]
    public float coilBSpeed = 0f;// не работает 

    //[Header("Snapping")]
    [SerializeField]
    bool snapToA = true;

    [SerializeField]
    bool snapToB = true;

    public bool isSnapToA { get { return snapToA; } }
    public bool isSnapToB { get { return snapToB; } }

    //[Header("Random")]
    [SerializeField]
    ChainMode chainMode;

    [SerializeField]
    int prefabId;//ind префаба из prefabsList. создаёт цепь из этого числа при режиме ChainMode.None

    [SerializeField]
    int queueOffset;

    [Header("Offsets")]
    public Vector2 offsetA;
    public Vector2 offsetB;

    //[Header("Scale Factor")]
    [SerializeField]
    float scaleFactorXY = 1; // для эдитора общее значение для размера икс и игрек

    [SerializeField]
    bool autoAspectRatio = true; // для эдитора, вкл/выкл соотношение пропорций

    //[Range(0.01f, 10f)]
    [SerializeField]
    float scaleFactorX = 1;

    //[Range(0.01f, 10f)]
    [SerializeField]
    float scaleFactorY = 1;


    //[Header("Other")]
    [SerializeField]
    List<GameObject> prefabsList;
    public List<GameObject> getPrefabList
    {
        get { return prefabsList; }
        set { prefabsList = value; }
    }

    [SerializeField]
    List<GameObject> prefabListClone = new List<GameObject>(); // нужен для явной провери в эдиторе, что оригинальный массив был изменён. Так как GUI ловит даже просто сворачивание массива и перестраивает цепь, а это не нужно

    [SerializeField]
    List<LinkInfo> links = new List<LinkInfo>();
    public int linksCount { get { return links.Count; } }

    [SerializeField]
    List<float> _prefabsListWidths = new List<float>();

    [SerializeField]
    public RigidBody2DSettings rbSettings;

    //включает/выключает проверку разрыва цепи в инспекторе
    public bool enableBreakCheck = true;

    //обновление проверки разрыва цепи в секундах
    public float updateBreakTick = 0.5f;
    float lastBreakTickTime = 0;


    //правая граница последнего звена, нужна для обновления звеньев в эдит режиме, если между А и В изменилось расстояние
    float lastRightBorder = 0;
    int currentId;

    //сюда записывается последне положение А или В. И в эдит режиме, цепь обновляется только, если А или В были сдвинуты
    Vector3 oldPosA = Vector3.zero;
    Vector3 oldPosB = Vector3.zero;
    float oldRotZ_A = 0;
    float oldRotZ_B = 0;

    // переменные для Editor
    public float handleSize = 1;
    public bool showPlaySettings = true;
    public bool showCreateSettings = true;

    //свойство контроля некоторыми параметрми цепи в игровом режиме для пользотеля
    public ChainController chainController { get; private set; }

    void Awake()
    {
        chainController = new ChainController(this);
    }

    void Start()
    {
        subscribeLinksFromEditOnRemoveLinkHandler();
    }

    //RESET если сброс настроек, то заново надо найти теповые концы и удалить звенья. Все массивы отчищаются автоматическыи.
    void Reset()
    {
        Transform findTempA = transform.Find("tempA");
        Transform findTempB = transform.Find("tempB");
        if (findTempA != null)
        {
            _tempA = findTempA.gameObject;
            A = _tempA;
            resetA();
        }
        if (findTempB != null)
        {
            _tempB = findTempB.gameObject;
            B = _tempB;
            resetB();
        }

        // удалить всех детей (звенья), кроме темповых концов
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child != _tempA && child != _tempB)
            {
                DestroyImmediate(child);
                i--;
            }
        }

    }

    // подписать все звенья, которые были созданы в эдиторе на удаление 
    void subscribeLinksFromEditOnRemoveLinkHandler()
    {
        Link link;
        for (int i = 0; i < links.Count; i++)
        {
            link = links[i].obj.GetComponent<Link>();
            if (link.removeMeFromList == null) link.removeMeFromList += removeLinkHandler;
        }
    }

    // переменные ниже используются в эдиторе, поэтому чтобы не возникало сообщение о неиспользуемых свойствах, написал заглушку
    void stopWarningMessages()
    {
        float tempFloat = anchorOffsetDummy;
        tempFloat = scaleFactorXY;
        tempFloat = tempFloat - 1;
        bool temp = autoAspectRatio;
        temp = !temp;
    }

    void Update()
    {
        //-------------edit mode
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            //    if (reset)
            //    {
            //        reset = false;
            //        rebuildRope();
            //        Debug.Log("reset");
            //    }
            if (Selection.activeGameObject == gameObject) return;

            //проверить А и В на существование (для эдит режиме)
            if (A == null || B == null)
            {
                checkABObjects();
            }

            if (A.transform.position != oldPosA || B.transform.position != oldPosB || A.transform.rotation.z != oldRotZ_A || B.transform.rotation.z != oldRotZ_B)
            {
                //Debug.Log("update");

                oldPosA = A.transform.position;
                oldPosB = B.transform.position;
                oldRotZ_A = A.transform.rotation.z;
                oldRotZ_B = B.transform.rotation.z;
                mainEditorUpdate();
            }
            return;
        }
#endif
        //-------------play mode
        coilACheck();
        coilBCheck();

        updateBreak();
    }

    // Катушка работает только, если есть привязка к А
    void coilACheck()
    {
        //если скорость нулевая, то нет смысла что-то там сложно просчитывать
        if (coilASpeed == 0 || prefabsList.Count == 0 || A == null) return;

        LinkInfo firstPartDragged;
        LinkInfo secondPart;

        // ОТМОТКА от А
        float coilASpeedReduсer = coilASpeed * 0.01f; // чтобы скорость в инспекторе не выставлялась такой большой, работаю с уменьшеной копии оригинальной скорости.
        if (coilASpeedReduсer > 0) //нужно двигать от А
        {
            //если отматывать нечего, или первое звено уже не прецепленно к А - то создаём новое звено и завершаем метод
            if (links.Count == 0 || links[0].hingleJoint_1.connectedBody != A.GetComponent<Rigidbody2D>())
            {
                if (chainMode == ChainMode.None) currentId = prefabId;
                else if (chainMode == ChainMode.Random) currentId = links.Count == 0 ? UnityEngine.Random.Range(0, prefabsList.Count) : links[0].prefabIdPrev;
                else if (chainMode == ChainMode.Queue) currentId = (links.Count + queueOffset) % prefabsList.Count;
                addPartFromA(currentId);
                return;
            }

            firstPartDragged = links[0];

            //направление от А до левой стороны звена (тащим ведь за левую сторону)
            Vector3 dir = firstPartDragged.obj.transform.TransformPoint(Vector3.left * firstPartDragged.widthHalf) - A.transform.TransformPoint(offsetA);

            float dist = dir.magnitude;
            dist += coilASpeedReduсer;

            //если второе звено, привязано к первому (которое тащим), то тащить в сторону ко второму звену
            if (links.Count > 1 && links[1].hingle1_IsActiveAndConnectedTo(firstPartDragged.rb2D))
            {
                firstPartDragged.hingleJoint_1.connectedAnchor += (Vector2)(A.transform.InverseTransformDirection(links[1].obj.transform.position - firstPartDragged.obj.transform.position).normalized) * coilASpeedReduсer;
            }
            //иначе тащить его вниз от А
            else firstPartDragged.hingleJoint_1.connectedAnchor = (Vector3)offsetA + (Vector3.down * dist);

            float w = (_prefabsListWidths[firstPartDragged.prefabIdPrev] + anchorOffset) * scaleFactorX;

            // СОЗДАТЬ ЛИ НОВОЕ? Если звено дальше от А, чем ширина нового звена (которое появится), то создать новое звено
            if (dist >= w)
            {
                //создать новое звено и прецепить к А серединой
                addPartFromA(firstPartDragged.prefabIdPrev);
            }
        }
        // НАМОТКА на А
        else if (coilASpeedReduсer < 0)
        {
            // если звеньев нет, или первое звено не привязано к А, то завершаем метод
            if (links.Count == 0 || links[0].hingle1_IsActiveAndConnectedTo(A.GetComponent<Rigidbody2D>()) == false) return;

            //первое звено (которое тащим)
            firstPartDragged = links[0];

            //расстояние и направление от А, до крепления первого звера
            Vector3 dir = firstPartDragged.hingleJoint_1.connectedAnchor - offsetA;
            float dist = dir.magnitude;

            //уменьшить расстояние, но не на больше, чем само расстояние
            dist += coilASpeedReduсer > dist ? dist : coilASpeedReduсer;

            // уменьшить расстояние от первого звена до А
            firstPartDragged.hingleJoint_1.connectedAnchor = (Vector3)offsetA + (dir.normalized * dist);

            if (dist <= 0.01f)
            {

                // прицепить следующее звено к А, только если след.звено было прикреплено к текущему (удалённому)
                if (links.Count > 1 && links[1].hingle1_IsActiveAndConnectedTo(firstPartDragged.rb2D))
                {
                    //второе звено
                    secondPart = links[1];
                    secondPart.hingleJoint_1.connectedBody = A.GetComponent<Rigidbody2D>();
                    //прицепить звено левой стороной к А, на текущем расстоянии
                    secondPart.hingleJoint_1.connectedAnchor = A.transform.InverseTransformPoint(secondPart.obj.transform.TransformPoint(Vector3.left * secondPart.widthHalf));
                }

                Destroy(firstPartDragged.obj);
                links.RemoveAt(0);
            }
        }
    }

    // такая же катушка, только для В
    void coilBCheck()
    {
        //если скорость нулевая, то нет смысла что-то там сложно просчитывать
        if (coilBSpeed == 0 || prefabsList.Count == 0 || B == null) return;


        // ОТМОТКА от B
        float coilВSpeedReduсer = coilBSpeed * 0.01f; // чтобы скорость в инспекторе не выставлялась такой большой, работаю с уменьшеной копией оригинальной скорости.
        if (coilВSpeedReduсer > 0) //нужно двигать от B
        {
            //если отматывать нечего, или последнее звено не присоеденено к В - то создаём новое звено и завершаем метод
            if (links.Count == 0 || links[links.Count - 1].hingle2_IsExistActiveAndConnectedTo(B.GetComponent<Rigidbody2D>()) == false)
            {
                if (chainMode == ChainMode.None) currentId = prefabId;
                else if (chainMode == ChainMode.Random) currentId = links.Count == 0 ? UnityEngine.Random.Range(0, prefabsList.Count) : links[links.Count - 1].prefabIdNext;
                else if (chainMode == ChainMode.Queue) currentId = (links.Count + queueOffset) % prefabsList.Count;
                addPartFromB(currentId);
                return;
            }

            LinkInfo lastPart = links[links.Count - 1];


            //направление от B до правой стороны последнего звена (тащим ведь за правую сторону)
            Vector3 dir = lastPart.obj.transform.TransformPoint(Vector3.right * lastPart.widthHalf) - B.transform.TransformPoint(offsetB);

            //Vector3 dir = lastPart.hingleJoint_2.connectedAnchor - offsetB;
            float dist = dir.magnitude;
            dist += coilВSpeedReduсer;


            //если звено одно, то тащить его вниз от B, иначе тащить в сторону к предыдущему звену, если текущее звено привязано к нему
            if (links.Count > 1 && lastPart.hingle1_IsActiveAndConnectedTo(links[links.Count - 2].rb2D))
            {
                lastPart.hingleJoint_2.connectedAnchor += (Vector2)(B.transform.InverseTransformDirection(links[links.Count - 2].obj.transform.position - lastPart.obj.transform.position).normalized) * coilВSpeedReduсer;
            }
            else lastPart.hingleJoint_2.connectedAnchor = (Vector3)offsetB + (Vector3.down * dist);


            float w = (_prefabsListWidths[lastPart.prefabIdNext] + anchorOffset) * scaleFactorX;

            // СОЗДАТЬ ЛИ НОВОЕ? Если звено дальше от А, чем ширина нового звена (которое появится), то создать новое звено
            if (dist >= w)
            {
                //создать новое звено и прецепить к А серединой
                addPartFromB(lastPart.prefabIdNext);
            }
        }
        // НАМОТКА на В
        else if (coilВSpeedReduсer < 0)
        {

            // если звеньев нет, или последнее звено не привязано к В, то тащить нечего..
            if (links.Count == 0 || links[links.Count - 1].hingle2_IsExistActiveAndConnectedTo(B.GetComponent<Rigidbody2D>()) == false) return;

            LinkInfo lastPart = links[links.Count - 1];
            //расстояние и направление от В, до крепления последнего звена
            Vector3 dir = lastPart.hingleJoint_2.connectedAnchor - offsetB;
            float dist = dir.magnitude;

            //уменьшить расстояние, но не на больше, чем само расстояние
            dist += coilВSpeedReduсer > dist ? dist : coilВSpeedReduсer;

            // уменьшить расстояние от последнего звена до В
            lastPart.hingleJoint_2.connectedAnchor = (Vector3)offsetB + (dir.normalized * dist);

            if (dist <= 0.01f)
            {

                // если звеньев было несколько и то, которое тащили было привязано к другому
                if (links.Count > 1 && lastPart.hingle1_IsActiveAndConnectedTo(links[links.Count - 2].rb2D))
                {
                    //предпоследнее звено
                    LinkInfo penultimatePart = links[links.Count - 2];

                    // прицепить педпоследнее звено к В вторым джоинтом.
                    if (penultimatePart.hingleJoint_2 == null)
                        penultimatePart.hingleJoint_2 = penultimatePart.obj.AddComponent<HingeJoint2D>();


                    //прицепить предпоследнее звено вторым джоинтом к В, на текущем расстоянии
                    penultimatePart.hingleJoint_2.connectedBody = B.GetComponent<Rigidbody2D>();

                    penultimatePart.hingleJoint_2.connectedAnchor = B.transform.InverseTransformPoint(penultimatePart.obj.transform.TransformPoint(Vector3.right * penultimatePart.widthHalf));

                    penultimatePart.hingleJoint_2.anchor = Vector3.right * penultimatePart.widthHalf;
                }

                Destroy(lastPart.obj);
                links.RemoveAt(links.Count - 1);
            }
        }
    }

    // когда от А отматывается достаточно, то это добавляет звено в начало и цепляет предыдущее к нему
    void addPartFromA(int prefabId)
    {
        Vector3 posA = A.transform.TransformPoint(offsetA);

        //направление нового звена: если звенья есть и первое звено прикрепленно к А, то повернуть к первому звену, иначе вправо от А
        Vector3 dirToFirstChain;
        if (links.Count > 0 && links[0].hingle1_IsActiveAndConnectedTo(A.GetComponent<Rigidbody2D>()))
            dirToFirstChain = (links[0].obj.transform.position - posA).normalized;
        else dirToFirstChain = Vector3.right;

        //создаём новое звено и поворачиваем его либо на след.звено, либо вправо (так красивее).
        LinkInfo p = createBasicPart(prefabId, false);
        p.obj.transform.position = posA + (dirToFirstChain * p.widthScaledHalf);
        p.obj.transform.right = dirToFirstChain;

        // прицепить ещё первое звено к новому
        if (links.Count != 0 && links[0].hingle1_IsActiveAndConnectedTo(A.GetComponent<Rigidbody2D>()))
        {
            links[0].hingleJoint_1.connectedBody = p.rb2D;
            links[0].hingleJoint_1.connectedAnchor = Vector3.right * (p.widthHalf + (anchorOffset * 0.5f));
            links[0].hingleJoint_1.anchor = Vector3.left * (links[0].widthHalf + (anchorOffset * 0.5f));
        }

        // прицепить новое звено к А
        p.hingleJoint_1.connectedBody = A.transform.GetComponent<Rigidbody2D>();
        p.hingleJoint_1.connectedAnchor = offsetA;
        p.hingleJoint_1.anchor = Vector3.left * p.widthHalf;

        links.Insert(0, p);
    }

    // когда от В отматывается достаточно, то это добавляет звено от В и цепляет предыдущее к нему, если нужно
    void addPartFromB(int prefabId)
    {
        Vector3 posB = B.transform.TransformPoint(offsetB);

        //направление В к последнему звену если оно есть и прикрепленно hingleJoint_2 к В , либо вправо от В
        Vector3 dirToFirstChain;
        if (links.Count > 0 && links[links.Count - 1].hingle2_IsExistActiveAndConnectedTo(B.GetComponent<Rigidbody2D>()))
            dirToFirstChain = (posB - links[links.Count - 1].obj.transform.position).normalized;
        else dirToFirstChain = Vector3.right;

        //создаём новое звено и поворачиваем его либо на B, либо вправо (так красивее).
        LinkInfo p = createBasicPart(prefabId);
        p.obj.transform.position = posB - (dirToFirstChain * p.widthScaledHalf);
        p.obj.transform.right = dirToFirstChain;

        //если звенья есть и последнее звено было прикрепленно к В вторым джоинтом
        if (links.Count != 0 && links[links.Count - 1].hingle2_IsExistActiveAndConnectedTo(B.GetComponent<Rigidbody2D>()))
        {
            //удалить у последнего звена второй hingleJoint_2
            if (links[links.Count - 1].hingleJoint_2 != null) Destroy(links[links.Count - 1].hingleJoint_2);

            // если последнее звено небыло привязано ни к чему, то выключить у него hingleJoint_1
            if (links[links.Count - 1].hingleJoint_1.connectedBody == null)
                links[links.Count - 1].hingleJoint_1.enabled = false;

            // прицепить новое звено к последнему первым hingleJoint_1 
            p.hingleJoint_1.connectedBody = links[links.Count - 1].rb2D;
            p.hingleJoint_1.connectedAnchor = Vector3.right * (links[links.Count - 1].widthHalf + (anchorOffset * 0.5f));
            p.hingleJoint_1.anchor = Vector3.left * (p.widthHalf + (anchorOffset * 0.5f));
        }
        //иначе выключить у нового звена первый джоинт (чтобы болтался)
        else p.hingleJoint_1.enabled = false;

        //создать второй hingleJoint_2 и привязаться к B
        p.hingleJoint_2 = p.obj.AddComponent<HingeJoint2D>();
        p.hingleJoint_2.connectedBody = B.GetComponent<Rigidbody2D>();
        p.hingleJoint_2.connectedAnchor = offsetB;
        p.hingleJoint_2.anchor = Vector3.right * p.widthHalf;

        links.Add(p);
    }

    // вызывается из эдитора при создании компонента Chain. Создаёт цепь, концы которых зависят от текущего выеделения объектов
    public void init()
    {
        createTempA();
        createTempB();

        prefabsList = new List<GameObject>();
        chainMode = ChainMode.None;

        rbSettings = new RigidBody2DSettings();

        //---------------------------

        //если выделен один объект, то это будет А, если два объекта, то А и Б, иначе по-умолчанию, пустышки
#if UNITY_EDITOR

        Transform[] selection = Selection.transforms;
        if (selection.Length == 1) defineABObjects(selection[0].gameObject);
        else if (selection.Length == 2)
        {
            int indSecond = Array.IndexOf(Selection.transforms, Selection.activeTransform);
            int indFirst = indSecond == 1 ? 0 : 1;
            defineABObjects(selection[indFirst].gameObject, selection[indSecond].gameObject);
        }
        else defineABObjects();

        Selection.activeGameObject = gameObject;
#endif

    }

    // создаёт темповый А
    void createTempA()
    {
        _tempA = new GameObject("tempA");
        _tempA.AddComponent<Rigidbody2D>().isKinematic = true;
        _tempA.transform.SetParent(transform);
    }

    // создаёт темповый В
    void createTempB()
    {
        _tempB = new GameObject("tempB");
        _tempB.AddComponent<Rigidbody2D>().isKinematic = true;
        _tempB.transform.SetParent(transform);
    }

    // определить объекты А и В, если не передать параметры, то вместо А и В будут темповые
    public void defineABObjects(GameObject a = null, GameObject b = null)
    {
#if UNITY_EDITOR

        if (a == null) resetA();
        else
        {
            A = a;
            oldPosA = A.transform.position;
        }


        if (b == null) resetB();
        else
        {
            B = b;
            oldPosB = B.transform.position;
        }
#endif
    }

    //проверить А и В на существование. Если их нет, то цепляет к темповымАВ. Когда в эдиторе удалить А или В, чтобы конец цепи остался на месте
    public void checkABObjects()
    {
        // если пользователь случайно удалил темповые А и В, то воссоздать их.
        if (_tempA == null) createTempA();
        if (_tempB == null) createTempB();


        if (A == null)
        {
            A = _tempA;
            A.transform.position = oldPosA;
        }
        else oldPosA = A.transform.position;

        if (B == null)
        {
            B = _tempB;
            B.transform.position = oldPosB;
        }
        else oldPosB = B.transform.position;

    }

    // сброс для А, устанавливает темповыйА в верхней половине экрана
    public void resetA()
    {
        A = _tempA;

#if UNITY_EDITOR

        bool isOrthographic = SceneView.lastActiveSceneView.camera.orthographic;
        if (isOrthographic) A.transform.position = (Vector2)SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.8f, 0)).GetPoint(0);
        else A.transform.position = Vector3.up * 3;
#endif
    }

    // сброс для В, устанавливает темповыйА в нижней половине экрана
    public void resetB()
    {
        B = _tempB;

#if UNITY_EDITOR

        bool isOrthographic = SceneView.lastActiveSceneView.camera.orthographic;
        if (isOrthographic) B.transform.position = (Vector2)SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.2f, 0)).GetPoint(0);
        else B.transform.position = Vector3.up * -3;
#endif
    }

    //получить оригинальную ширину каждого префаба
    void defineOriginalSizeOfPrefabs()
    {
        _prefabsListWidths.Clear();
        if (prefabsList != null && prefabsList.Count != 0)
        {
            for (int i = 0; i < prefabsList.Count; i++)
            {
                //надо собрать все боундсы детей, ведь возможно префаб пустой и его нельзя добавлять
                Bounds allBounds = new Bounds();
                Renderer[] childrensRend = prefabsList[i].GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in childrensRend)
                {
                    allBounds.Encapsulate(rend.bounds);
                }

                if (allBounds.size == Vector3.zero)
                {
                    prefabsList.RemoveAt(i--);
                    return;
                }
                _prefabsListWidths.Add(allBounds.size.x);
            }
        }
    }

    // обновление верёвки, при изменении её свойств в Edit режиме
    public void mainEditorUpdate()
    {
        if (prefabsList == null || prefabsList.Count == 0) return;

        //проверить существование звеньев и пересоздать цепь, если некоторые звенья не нашлись, возможно пользователь удалил их в иерархии
        checkForDeleteLinksInHyerarchy();

        Vector3 posA = A.transform.TransformPoint(offsetA);
        Vector3 posB = B.transform.TransformPoint(offsetB);

        Vector3 dir = (posB - posA);
        float dist = dir.magnitude;
        dir = dir.normalized;

        // пересчитать lastRightBorder и границы звеньев, 
        // так как если А и В не двигались, НО изменялся scaleFactor, нужно узнать новые границы
        // и в зависимости от них уже добавлять или удалять звенья
        updateScaleParts();
        updateLastRightBorderAndPartsBorder();

        //добавить звено если нужно вызывает автоматом updateLastRightBorderAndPartsBorder
        while (lastRightBorder < dist) addNewParts();

        //удалить звенья если нужно вызывает автоматом updateLastRightBorderAndPartsBorder
        removePartsIfNeeded(dist);

        //выстоить цепочку
        updatePositions(posA, dir);

        //обновить положение креплений между звеньями
        updateHindleJoints2D();

        //обновить привязки и динамику объектов привязки
        updateSnappingToAB();
    }

    // добавляет новое звено в массив
    void addNewParts()
    {
        if (chainMode == ChainMode.None) currentId = prefabId;
        //если выбран Рандом, то для первого звена делаем случайный ID, иначе берём prefabIdNext с предыдущего звена в цепочке (последнего в массиве)
        else if (chainMode == ChainMode.Random) currentId = links.Count == 0 ? UnityEngine.Random.Range(0, prefabsList.Count) : links[links.Count - 1].prefabIdNext;
        else if (chainMode == ChainMode.Queue) currentId = (links.Count + queueOffset) % prefabsList.Count;

        LinkInfo p = createBasicPart(currentId);

        links.Add(p);

        //после добавления, надо бы пересчитать все границы и обновить lastRightBorder
        updateLastRightBorderAndPartsBorder();
    }

    // проверяет, если звенья выходят за пределы текущей длинны, то удаляет эвенья
    void removePartsIfNeeded(float dist)
    {
        for (int i = links.Count - 1; i >= 0; i--)
        {
            LinkInfo p = links[i];
            if (p.rightBorder > dist && links.Count > 1)
            {
                DestroyImmediate(p.obj);
                links.RemoveAt(i);
            }
        }
        //если было удаление, то нужно пересчитать границы звеньев и найти крайнуюю границу последнего звена
        updateLastRightBorderAndPartsBorder();
    }

    // обновить масштаб звена и его свойства widthScaled widthScaledHalf
    void updateScaleParts()
    {
        for (int i = 0; i < links.Count; i++)
            links[i].updateScaleAndScaleProps(scaleFactorX, scaleFactorY);
    }

    // обновить позиции (выстоить цепочку) и масштаб
    void updatePositions(Vector3 posA, Vector3 dir)
    {
        for (int i = 0; i < links.Count; i++)
        {
            LinkInfo p = links[i];

            if (i == 0) p.obj.transform.position = posA + (dir * p.widthScaledHalf);
            else
                p.obj.transform.position = links[i - 1].obj.transform.position + dir * (links[i - 1].widthScaledHalf + (p.widthScaledHalf + (anchorOffset * scaleFactorX)));

            p.obj.transform.right = dir;
        }
    }

    // обновляет позицию крепления HingeJoint2D между звеньями.
    // в последнем звене создаёт второй HingeJoint2D для крепления к B (если нужно будет).
    // удаляет второй HingeJoint2D если он есть не на последнем звене.
    void updateHindleJoints2D()
    {
        LinkInfo p;
        for (int i = 0; i < links.Count; i++)
        {
            p = links[i];

            //удалить второй HingeJoint2D, если он есть
            if (p.hingleJoint_2 != null) DestroyImmediate(p.hingleJoint_2);

            // обновить hindleJoints привязать к пред.звену
            if (i > 0)
            {
                LinkInfo preP = links[i - 1];
                p.hingleJoint_1.connectedBody = preP.rb2D;
                p.hingleJoint_1.autoConfigureConnectedAnchor = false;
                p.hingleJoint_1.connectedAnchor = Vector3.right * (preP.widthHalf + ((anchorOffset * 0.5f)));
                p.hingleJoint_1.anchor = Vector3.left * (p.widthHalf + ((anchorOffset * 0.5f)));
            }
        }

        // добавить (второй) HingeJoint2D в последнее звено
        if (links.Count != 0)
        {
            p = links[links.Count - 1];
            p.hingleJoint_2 = p.obj.AddComponent<HingeJoint2D>();
        }
    }

    // обновить привязки к началу и концу и установка вкл/выкл динамики у A и B
    void updateSnappingToAB()
    {
        LinkInfo p;

        if (snapToA)
        {
            Rigidbody2D rbA = A.GetComponent<Rigidbody2D>();
            // если у цепляемого объекта небыло твёродого тела, до добавить его, но сделать кинематическим
            if (rbA == null) { rbA = A.AddComponent<Rigidbody2D>(); rbA.isKinematic = true; }

            p = links[0];

            p.hingleJoint_1.enabled = true;
            p.hingleJoint_1.connectedBody = A.GetComponent<Rigidbody2D>();
            p.hingleJoint_1.connectedAnchor = offsetA;
            p.hingleJoint_1.anchor = Vector3.left * p.widthHalf;
        }
        else
            links[0].hingleJoint_1.enabled = false;


        if (snapToB)
        {
            Rigidbody2D rbB = B.GetComponent<Rigidbody2D>();
            // если у цепляемого объекта небыло твёродого тела, до добавить его, но сделать кинематическим
            if (rbB == null) { rbB = B.AddComponent<Rigidbody2D>(); rbB.isKinematic = true; }

            p = links[links.Count - 1];

            p.hingleJoint_2.enabled = true;
            p.hingleJoint_2.connectedBody = B.GetComponent<Rigidbody2D>();
            p.hingleJoint_2.connectedAnchor = offsetB;
            p.hingleJoint_2.anchor = Vector3.right * p.widthHalf;
        }
        else
            links[links.Count - 1].hingleJoint_2.enabled = false;
    }

    // обновление последней границы в цепочке lastRightBorder и leftBorder/rightBorder звеньев
    // ВАЖНЫЙ МЕТОД, долежн вызываться после изменения scaleFactor и после создания/удаления звеньев
    void updateLastRightBorderAndPartsBorder()
    {
        lastRightBorder = 0;
        LinkInfo p;
        for (int i = 0; i < links.Count; i++)
        {
            p = links[i];

            if (i == 0)
            {
                lastRightBorder += links[i].widthScaled;

                p.leftBorder = 0;
                p.rightBorder = p.widthScaled;
            }
            else
            {
                p.leftBorder = lastRightBorder;
                p.rightBorder = lastRightBorder + p.widthScaled + (anchorOffset * scaleFactorX);

                lastRightBorder += links[i].widthScaled + (anchorOffset * scaleFactorX);
            }

        }
        // для последней границы добавляем ширину следующего звена, которое создастся, чтобы новое звено появлялось только тогда,
        // когда между правой границей последнего звена и последней точкой - было расстоянеие в которое влезет новое звено
        if (links.Count > 0)
        {
            // ширина звена которое создастся следующим
            float nextLinkWidthInFuture = _prefabsListWidths[links[linksCount - 1].prefabIdNext];
            lastRightBorder += (nextLinkWidthInFuture * scaleFactorX) + (anchorOffset * scaleFactorX);

        }
    }

    // удаляет массив цепи и массив префабов
    void clearAllListsAndRemoveGOs()
    {
        foreach (var part in links)
            DestroyImmediate(part.obj);

        links.Clear();
        //------------------------------
        if (prefabsList != null && prefabsList.Count != 0)
        {
            for (int i = prefabsList.Count - 1; i >= 0; i--)
            {
                if (prefabsList[i] == null) prefabsList.RemoveAt(i);
            }
        }

        prefabListClone.Clear();

    }

    // пересоздать верёвку с нуля ВАЖНЫЙ МЕТОД
    public void rebuildRope()
    {
        clearAllListsAndRemoveGOs();

        //------------------------------
        lastRightBorder = 0;
        defineOriginalSizeOfPrefabs();
        makePrefabListClone();

        mainEditorUpdate();

    }

    // нужно отследить явное изменение массива с префабами, так как GUI ловит даже просто сворачивание массива в инспекторе.
    // поэтому делаю копию массива префабов, а потом сравниваю с оригиналом, если они разные, значит оригинальный массив с префабами изменился

    //создать копию массива префабов
    public void makePrefabListClone()
    {
        prefabListClone.Clear();

        for (int i = 0; i < prefabsList.Count; i++)
            prefabListClone.Add(prefabsList[i]);
    }

    // сравнить копию массива с оригиналом, если они разные, то вернётся true
    public bool prefabListAndCloneIsDifferent()
    {
        if (prefabsList.Count != prefabListClone.Count) return true;

        for (int i = 0; i < prefabsList.Count; i++)
            if (ReferenceEquals(prefabsList[i], prefabListClone[i]) != true) return true;

        return false;
    }

    // создаёт экземпляр PartInfo с настройками по-умолчанию.
    // toEnd - добавляем в конец цепи или в начало
    LinkInfo createBasicPart(int prefabId, bool toEnd = true)
    {
        LinkInfo p = new LinkInfo();
        p.obj = Instantiate<GameObject>(prefabsList[prefabId]);
        p.prefabId = prefabId;

        if (chainMode == ChainMode.None) p.prefabIdNext = p.prefabIdPrev = p.prefabId;
        else if (chainMode == ChainMode.Queue)
        {
            p.prefabIdNext = p.prefabId + 1 == prefabsList.Count ? 0 : p.prefabId + 1;
            p.prefabIdPrev = p.prefabId - 1 < 0 ? prefabsList.Count - 1 : p.prefabId - 1;

        }
        else if (chainMode == ChainMode.Random)
        {
            if (toEnd)
            {
                // если звено первое, то в prefabIdPrev записываем случайно число, иначе id предыдущего звена (последнего в цепи)
                if (links.Count == 0) p.prefabIdPrev = UnityEngine.Random.Range(0, prefabsList.Count);
                else p.prefabIdPrev = links[linksCount - 1].prefabId;

                p.prefabIdNext = UnityEngine.Random.Range(0, prefabsList.Count);
            }
            else
            {
                p.prefabIdPrev = UnityEngine.Random.Range(0, prefabsList.Count);
                if (links.Count == 0) p.prefabIdNext = UnityEngine.Random.Range(0, prefabsList.Count);
                else p.prefabIdNext = links[0].prefabId;
            }
        }

        p.width = _prefabsListWidths[prefabId];
        p.widthHalf = p.width / 2;
        p.widthScaled = p.width * scaleFactorX;
        p.widthScaledHalf = p.widthScaled / 2;

        p.obj.transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1);

        p.rb2D = p.obj.GetComponent<Rigidbody2D>() == null ? p.obj.AddComponent<Rigidbody2D>() : p.obj.GetComponent<Rigidbody2D>();
        p.hingleJoint_1 = p.obj.AddComponent<HingeJoint2D>();
        p.obj.AddComponent<Link>().removeMeFromList += removeLinkHandler;   //подписка на удаление, но если это звено было создано в эдиторе, 

        p.obj.transform.SetParent(this.transform);

        return p;
    }

    // Вызывается когда звено удалилось из игры. Оно так же удаляется из массива links
    void removeLinkHandler(GameObject go)
    {
        for (int i = 0; i < links.Count; i++)
        {
            if (links[i].obj == go)
            {
                go.GetComponent<Link>().removeMeFromList -= removeLinkHandler;
                links.RemoveAt(i);
                break;
            }
        }
    }

    // перепибает все звенья и изменяем настройки твёрдого тела
    public void updateChainSettingsRB2D()
    {
        for (int i = 0; i < links.Count; i++)
        {
            Rigidbody2D rb = links[i].obj.GetComponent<Rigidbody2D>();
            rb.mass = rbSettings.mass;
            rb.drag = rbSettings.linearDrag;
            rb.angularDrag = rbSettings.angularDrag;
            rb.gravityScale = rbSettings.grav;
        }

    }

    // устанавливаем по-умолчанию настройки твёрдого тела для цепи
    public void setSettingsRBDefault()
    {
        rbSettings.mass = 1;
        rbSettings.linearDrag = 0;
        rbSettings.angularDrag = 0.05f;
        rbSettings.grav = 1f;

        rbSettings.breakDistance = 3f;
        rbSettings.breakDistanceSqrt = 3f * 3f;

        updateChainSettingsRB2D();

        updateBreakTick = 0.5f;

    }

    // проверка на возможность разрыва цепи. Если расстояние между звеньями больше чем лимит, то идёт разрыв там, где это расстояние самое большое.
    void updateBreak()
    {
        if (!enableBreakCheck) return;

        if (Time.time > lastBreakTickTime + updateBreakTick) lastBreakTickTime = Time.time;
        else return;

        HingeJoint2D h;

        int maxInd = -1;    //индекс звена, у которого будем максимальное расстояние между якорями
        float maxMag = 0;   //записывает самое большое расстояние
        Vector3 globAnchor;
        Vector3 globConnectedAnchor;

        //пробежать по звеньям и найти то звено, у которого самое большое растояние (больше чем limit) между якорями 
        //проверяем со второго звена, чтобы при перетаскивании А, цепь не разлеталась
        for (int i = 1; i < links.Count; i++)
        {
            h = links[i].hingleJoint_1;
            if (h.isActiveAndEnabled && h.connectedBody != null)
            {
                globAnchor = h.gameObject.transform.TransformPoint(h.anchor);
                globConnectedAnchor = h.connectedBody.gameObject.transform.TransformPoint(h.connectedAnchor);

                float sqrtDist = (globAnchor - globConnectedAnchor).sqrMagnitude;
                if (sqrtDist > rbSettings.breakDistance)
                {
                    if (maxMag < sqrtDist)
                    {
                        maxMag = sqrtDist;
                        maxInd = i;
                    }
                }
            }

        }
        //если нашли такое звено, то отключаем ему hingle
        if (maxInd != -1) links[maxInd].hingleJoint_1.enabled = false;

    }

    // проверка того, существуют ли все звенья на сцене. может быть такое, что пользователь удалил их в иерархии
    public void checkForDeleteLinksInHyerarchy()
    {
        if (links.Count == 0) return;

        for (int i = 0; i < links.Count; i++)
        {
            if (links[i].obj == null)
            {
                rebuildRope();
                break;
            }
        }
    }

    // Возвращает списко имём префабов без коллайдеров, ввызывается в Editor.
    public string namePrefabsWitoutColliders()
    {
        string mess = "";
        for (int i = 0; i < prefabsList.Count; i++)
        {
            if (prefabsList[i].GetComponentsInChildren<Collider2D>().Length == 0) mess += "\n-" + prefabsList[i].name;
        }
        return mess;
    }

    // отсоединить от А (во время игры)
    public void snapOffA()
    {
        if (Application.isPlaying == false) return;

        for (int i = 0; i < links.Count; i++)
        {
            if (links[i].hingle1_IsActiveAndConnectedTo(A.GetComponent<Rigidbody2D>()))
            {
                links[i].hingleJoint_1.enabled = false;
                break;
            }
        }
    }

    // отсоединить от В (во время игры)
    public void snapOffB()
    {
        if (Application.isPlaying == false) return;

        for (int i = 0; i < links.Count; i++)
        {
            if (links[i].hingle2_IsExistActiveAndConnectedTo(B.GetComponent<Rigidbody2D>()))
            {
                links[i].hingleJoint_2.enabled = false;
                break;
            }
        }
    }


    // статический метод, который определяет является ли переданный объект звеном
    // и если это звено, то отключает ему hingle (первый) 
    public static void CutMe(GameObject gameObject)
    {
        Link link = gameObject.GetComponent<Link>();
        if (link != null)
        {
            gameObject.GetComponent<HingeJoint2D>().enabled = false;
        }
    }
}


//Базовый класс звена, для простоты манипуляции со звеном.
[System.Serializable]
public class LinkInfo
{
    public GameObject obj;

    public float width;
    public float widthHalf;
    public float widthScaled;
    public float widthScaledHalf;

    public float leftBorder;
    public float rightBorder;

    public int prefabId = -1;
    public int prefabIdNext = -1;
    public int prefabIdPrev = -1;

    //public BoxCollider2D boxCollider2D;
    public Rigidbody2D rb2D;
    public HingeJoint2D hingleJoint_1 = null;
    public HingeJoint2D hingleJoint_2 = null;


    //задаёт мастштаб и пересчитывает widthScaled и widthScaledHalf и anchorOffset
    public void updateScaleAndScaleProps(float sX, float sY)
    {
        obj.transform.localScale = new Vector3(sX, sY, 1);
        widthScaled = width * sX;
        widthScaledHalf = widthScaled / 2;
    }

    //если hingleJoint_1 включен и привязан к твёрдому телу, возвращает true, иначе false
    public bool hingle1_IsActiveAndConnectedTo(Rigidbody2D rb2)
    {
        return (hingleJoint_1.enabled && hingleJoint_1.connectedBody == rb2);
    }

    //если hingleJoint_2 есть, активен и привязан к твёрдому телу то true, иначе false
    public bool hingle2_IsExistActiveAndConnectedTo(Rigidbody2D rb2)
    {
        return (hingleJoint_2 != null && hingleJoint_2.enabled && hingleJoint_2.connectedBody == rb2);
    }

}

//класс с настройками для твёрдого тела цепи
[System.Serializable]
public class RigidBody2DSettings
{
    public float mass = 1;
    public float linearDrag = 0;
    public float angularDrag = 0.05f;
    public float grav = 1;

    public float breakDistance = 3f;
    public float breakDistanceSqrt = 3f * 3f;
}
