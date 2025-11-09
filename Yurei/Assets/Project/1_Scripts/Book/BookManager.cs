using System;
using DG.Tweening;
using EditorAttributes;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;


public class BookManager : MonoBehaviour
{
    public static BookManager Instance;
 
    [Serializable]
    public struct DoublePageMaterialData
    {
        public Material leftPagesMaterial;
        public Material rightPagesMaterial;
        public bool isAccessible;
    }
    
    [GUIColor(GUIColor.Magenta)]
    [SerializeField] private bool progMode;

    [Space(5)]
    [GUIColor(GUIColor.Red)]
    [Header("Book")]
    [EnableField(nameof(progMode))] [SerializeField] private Material emptyPageMaterial;
    [EnableField(nameof(progMode))] [SerializeField] private GameObject pagePrefab;
    [EnableField(nameof(progMode))] [SerializeField] private Transform pageSpawnPoint;
    [EnableField(nameof(progMode))] [SerializeField] private Transform lookBookTransform;
    [EnableField(nameof(progMode))] [SerializeField] private SkinnedMeshRenderer bookBase;
    
    [Space(5)]
    [Header("DEBUG")]
    [GUIColor(GUIColor.Orange)]
    [EnableField(nameof(progMode))] public int numberTurningPageToLeft;
    [EnableField(nameof(progMode))] public int numberTurningPageToRight;
    [EnableField(nameof(progMode))] [SerializeField] private int currentRightPageNumber = 0;
    [EnableField(nameof(progMode))] [SerializeField] private int currentLeftPageNumber = 1;
    [EnableField(nameof(progMode))] [SerializeField] private int pageCount;
    [EnableField(nameof(progMode))] [SerializeField] private bool isLookingBook;
    [EnableField(nameof(progMode))] [SerializeField] private bool isCameraMoving;
    
    [Space(5)]
    [Header("Pages Materials")]
    [Tooltip("this is an array of pages : a page is defined by its right and left material")]
    [GUIColor(GUIColor.Cyan)]
    [SerializeField, DataTable(true)] 
    private DoublePageMaterialData[] doublePageMaterialDatas;

    

   
    
    
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private PlayerInputController _input;

    private void Awake() => Instance = this;

    private void Start()
    {
        pageCount = doublePageMaterialDatas.Length;
        
        SetMaterialRightPage(pageCount > 0 ? doublePageMaterialDatas[0].rightPagesMaterial : emptyPageMaterial);
        SetMaterialLeftPage(pageCount > 1 ? doublePageMaterialDatas[1].leftPagesMaterial : emptyPageMaterial);
        
        _input = PlayerInputController.Instance;
        if (_input == null)
        {
            Debug.LogWarning("PlayerInputController is null");
            return;
        }
        
        _input.OnLookBookPressed += HandleLookBook;
        _input.OnExitBookPressed += HandleExitBook;
        _input.OnNextPagePressed += HandleNextPage;
        _input.OnPreviousPagePressed += HandlePreviousPage;
    }

    /// <summary>
    /// Conditions functions
    /// </summary>
    private bool CanLookBook() => !isLookingBook && !isCameraMoving;
    private bool CanExitBook() => isLookingBook && numberTurningPageToLeft <= 0 && numberTurningPageToRight <= 0 && !isCameraMoving;
    private bool CanNextPage() => isLookingBook && numberTurningPageToLeft <= 0 && currentLeftPageNumber < pageCount - 1;
    private bool CanPreviousPage() => isLookingBook && numberTurningPageToRight <= 0 && currentRightPageNumber > 0;

    /// <summary>
    /// Handle functions
    /// </summary>
    private void HandleLookBook()
    {
        if (CanLookBook()) LookBook();
    }
    
    private void HandleExitBook()
    {
        if (CanExitBook()) ExitBook();
    }
    
    private void HandleNextPage()
    {
        if (CanNextPage()) NextPage();
    }
    
    private void HandlePreviousPage()
    {
        if (CanPreviousPage()) PreviousPage();
    }
    
    /// <summary>
    /// Logic functions
    /// </summary>
    private void LookBook(Action onComplete = null)
    {
        isCameraMoving = true;
            
        lastCameraPosition = CameraManager.Instance.mainCamera.transform.position;
        lastCameraRotation = CameraManager.Instance.mainCamera.transform.rotation;
        CameraManager.Instance.MoveCameraTo(lookBookTransform.position, lookBookTransform.rotation, Ease.InOutExpo, 0.5f,
            () =>
            {
                isCameraMoving = false;
                isLookingBook = true;
                onComplete?.Invoke();
            });
    }
    
    private void ExitBook()
    {
        isCameraMoving = true;
        isLookingBook = false;
        CameraManager.Instance.MoveCameraTo(lastCameraPosition, lastCameraRotation, Ease.InOutExpo, 0.5f, () =>
        {
            isCameraMoving = false;
        });
    }
    
    /// <summary>
    /// Turn to the next page
    /// steps :
    ///     - init page position to start
    ///     - copy "left" on "left of moving page"
    ///     - change "left" to the next left page 
    ///     - change " right of moving page" to the next right page
    ///     - rotate our page
    ///     - change "right" on "right of moving page"
    ///     - remove moving page
    /// </summary>
    private void NextPage(Action onComplete = null)
    {
        GameObject go = Instantiate(pagePrefab, pageSpawnPoint.position, pageSpawnPoint.rotation);
        if (go.TryGetComponent(out PageHelper page))
        {
            Material nextLeftMaterial = null;
            Material nextRightMaterial = null;
            int nextRightIndex = -1;
            int nextLeftIndex = -1;
            
            // Get element we'll need
            GetLeftPageMaterial(out var actualLeftMaterial);
            GetNextLeftPageMaterial(ref nextLeftMaterial, ref nextLeftIndex, out var isNextLeft);
            GetNextRightPageMaterial(ref nextRightMaterial, ref nextRightIndex, out var isNextRight);
            
            if (!isNextLeft || !isNextRight)
            { 
                Debug.LogWarning("Can't PreviousPage function : " + isNextLeft + "; " + isNextRight);
                Destroy(go);
                return;
            }
            
            currentLeftPageNumber = nextLeftIndex;
            currentRightPageNumber = nextRightIndex;
            numberTurningPageToRight++;
            
            // copy "left" on "left of moving page"
            page.SetMaterialLeftMovingPage(actualLeftMaterial);
            
            // change "left" to the next left page 
            SetMaterialLeftPage(nextLeftMaterial);
            
            // change "right of moving page" to the next right page
            page.SetMaterialRightMovingPage(nextRightMaterial);

            page.PlayPageAnimation(true, onComplete);
        }
        
        else Destroy(go);
    }

    /// <summary>
    /// Turn to the previous page
    /// steps :
    ///     - init page position to end
    ///     - copy "right" on "right of moving page"
    ///     - change "right" to the next right page 
    ///     - change " left of moving page" to the next left page
    ///     - rotate our page with reverse animation
    ///     - change "left" on "left of moving page"
    ///     - remove moving page
    /// </summary>
    private void PreviousPage(Action onComplete = null)
    {
        GameObject go = Instantiate(pagePrefab, pageSpawnPoint.position, pageSpawnPoint.rotation);
        if (go.TryGetComponent(out PageHelper page))
        {
            Material previousLeftMaterial = null;
            Material previousRightMaterial = null;
            int previousRightIndex = -1;
            int previousLeftIndex = -1;
            
            // Get element we'll need
            GetRightPageMaterial(out var actualRightMaterial);
            GetPreviousLeftPageMaterial(ref previousLeftMaterial, ref previousLeftIndex, out var isPreviousLeft);
            GetPreviousRightPageMaterial(ref previousRightMaterial, ref previousRightIndex, out var isPreviousRight);

            if (!isPreviousLeft || !isPreviousRight)
            { 
                Debug.LogWarning("Can't PreviousPage function : " + isPreviousLeft + "; " + isPreviousRight);
                Destroy(go);
                return;
            }
            
            currentRightPageNumber = previousRightIndex;
            currentLeftPageNumber = previousLeftIndex;
            numberTurningPageToLeft++;
            
            // copy "right" on "right of moving page"
            page.SetMaterialRightMovingPage(actualRightMaterial);
            
            // change "right" to the previous right page 
            SetMaterialRightPage(previousRightMaterial);
            
            // change " left of moving page" to the previous left page
            page.SetMaterialLeftMovingPage(previousLeftMaterial);

            page.PlayPageAnimation(false, onComplete);
        }
        
        else Destroy(go);
    }

    /// <summary>
    /// Helpers functions
    /// </summary>
    private void GetLeftPageMaterial(out Material mat) => mat = bookBase.materials[0] == null ? emptyPageMaterial : bookBase.materials[0];
    private void GetRightPageMaterial(out Material mat) => mat = bookBase.materials[2] == null ? emptyPageMaterial : bookBase.materials[2];
    public void SetMaterialLeftPage(Material material)
    {
        Material[] mats = bookBase.materials;
        mats[0] = material;
        bookBase.materials = mats;
    }

    public void SetMaterialRightPage(Material material)
    {
        Material[] mats = bookBase.materials;
        mats[2] = material;
        bookBase.materials = mats;
    }

    private void GetPreviousRightPageMaterial(ref Material mat, ref int indexPreviousPage, out bool isPreviousRight)
    {
        // error case
        if (currentRightPageNumber <= 0)
        {
            Debug.LogWarning("issue with GetPreviousRightPageMaterial function");
            isPreviousRight = false;
            return;
        }
        
        // get the previous accessible page
        for (int i = currentRightPageNumber - 1; i >= 0; i--)
        {
            if (doublePageMaterialDatas[i].isAccessible)
            {
                indexPreviousPage = i;
                mat = doublePageMaterialDatas[i].rightPagesMaterial == null ? emptyPageMaterial : doublePageMaterialDatas[i].rightPagesMaterial;
                isPreviousRight = true;
                return;
            }
        }
        
        // case there is no previous page that is accessible
        Debug.LogWarning("no previous accessible right page");
        isPreviousRight = false;
    }
    
    private void GetNextRightPageMaterial(ref Material mat, ref int indexNextPage, out bool isNextRight)
    {
        // error case
        if (currentRightPageNumber >= pageCount)
        {
            Debug.LogWarning("issue with GetNextRightPageMaterial function");
            isNextRight = false;
            return;
        }
        
        // get the previous accessible page
        for (int i = currentRightPageNumber + 1; i < pageCount; i++)
        {
            if (doublePageMaterialDatas[i].isAccessible)
            {
                indexNextPage = i;
                mat = doublePageMaterialDatas[i].rightPagesMaterial == null ? emptyPageMaterial : doublePageMaterialDatas[i].rightPagesMaterial;
                isNextRight = true;
                return;
            }
        }
        
        // case there is no previous page that is accessible
        Debug.LogWarning("no next accessible right page");
        isNextRight = false;
    }

    private void GetPreviousLeftPageMaterial(ref Material mat, ref int indexPreviousPage, out bool isPreviousLeft)
    {
        // error case
        if (currentLeftPageNumber <= 0)
        {
            Debug.LogWarning("issue with GetPreviousLeftPageMaterial function");
            isPreviousLeft = false;
            return;
        }
        
        // get the previous accessible page
        for (int i = currentLeftPageNumber - 1; i >= 0; i--)
        {
            if (doublePageMaterialDatas[i].isAccessible)
            {
                indexPreviousPage = i;
                mat = doublePageMaterialDatas[i].leftPagesMaterial == null ? emptyPageMaterial : doublePageMaterialDatas[i].leftPagesMaterial;
                isPreviousLeft = true;
                return;
            }
        }
        
        // case there is no previous page that is accessible
        Debug.LogWarning("no previous accessible left page");
        isPreviousLeft = false;
    }
    
    private void GetNextLeftPageMaterial(ref Material mat, ref int indexNextPage, out bool isNextLeft)
    {
        // error case
        if (currentLeftPageNumber >= pageCount)
        {
            Debug.LogWarning("issue with GetNextLeftPageMaterial function");
            isNextLeft = false;
            return;
        }
        
        // get the previous accessible page
        for (int i = currentLeftPageNumber + 1; i < pageCount; i++)
        {
            if (doublePageMaterialDatas[i].isAccessible)
            {
                indexNextPage = i;
                mat = doublePageMaterialDatas[i].leftPagesMaterial == null ? emptyPageMaterial : doublePageMaterialDatas[i].leftPagesMaterial;
                isNextLeft = true;
                return;
            }
        }
        
        // case there is no previous page that is accessible
        Debug.LogWarning("no next accessible left page");
        isNextLeft = false;
    }
    
    public int GetCurrentRightPageNumber() => currentRightPageNumber;
    public int GetCurrentLeftPageNumber() => currentLeftPageNumber;

    /// <summary>
    /// On Player finish page
    /// </summary>
    public void OnPlayerFinishedPage(bool isRightPage, Action onComplete = null)
    {
        LookBook(() =>
        {
            // after look book 
            isLookingBook = false;
            if (isRightPage) PreviousPage(onComplete);
            else NextPage(onComplete);
        });
    }
}