using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    public static BookManager Instance;
 
    [System.Serializable]
    public class DoublePageMaterialData
    {
        public Material rightPagesMaterial;
        public Material leftPagesMaterial;
    }
    
    [Space(5)]
    [Header("Pages Materials")]
    [Tooltip("this is an array of pages : a page is defined by its right and left material")]
    [SerializeField] private DoublePageMaterialData[] doublePageMaterialDatas;
    [SerializeField] private Material emptyPageMaterial;
    
    [Space(5)]
    [Header("Book")]
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] private Transform pageSpawnPoint;
    [SerializeField] private Transform lookBookTransform;
    [SerializeField] private SkinnedMeshRenderer bookBase;
    
    [Space(5)]
    [Header("DEBUG")]
    public int currentRightPageNumber = 0;
    public int currentLeftPageNumber = 1;
    public int pageCount;
    public int numberTurningPageToLeft;
    public int numberTurningPageToRight;
    public bool isLookingBook;
    public bool isCameraMoving;
    
    
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private PlayerInputController _input;

    private void Awake() => Instance = this;

    private void Start()
    {
        pageCount = doublePageMaterialDatas.Length - 1;
        
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
    private bool CanNextPage() => isLookingBook && numberTurningPageToLeft <= 0 && currentRightPageNumber < pageCount;
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
            // Get element we'll need
            GetLeftPageMaterial(out var actualLeftMaterial);
            GetNextLeftPageMaterial(out var nextLeftMaterial, out currentLeftPageNumber);
            GetNextRightPageMaterial(out var nextRightMaterial, out currentRightPageNumber);
            
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
            // Get element we'll need
            GetRightPageMaterial(out var actualRightMaterial);
            GetPreviousLeftPageMaterial(out var previousLeftMaterial, out currentLeftPageNumber);
            GetPreviousRightPageMaterial(out var previousRightMaterial, out currentRightPageNumber);

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
    private void GetLeftPageMaterial(out Material mat) => mat = bookBase.materials[0];
    private void GetRightPageMaterial(out Material mat) => mat = bookBase.materials[2];
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

    private void GetPreviousRightPageMaterial(out Material mat, out int indexPreviousPage)
    {
        mat = currentRightPageNumber > 0 ? doublePageMaterialDatas[currentRightPageNumber - 1].rightPagesMaterial : emptyPageMaterial;
        indexPreviousPage = currentRightPageNumber - 1;
    }
    
    private void GetNextRightPageMaterial(out Material mat, out int indexNextPage)
    {
        mat = currentRightPageNumber < pageCount ? doublePageMaterialDatas[currentRightPageNumber + 1].rightPagesMaterial : emptyPageMaterial;
        indexNextPage = currentRightPageNumber + 1;
    }

    private void GetPreviousLeftPageMaterial(out Material mat, out int indexNextPage) //////////////////MODIFIER POUR LE SYSTEME DE PAGE MANQUANTE
    {
        mat = currentLeftPageNumber > 0 ? doublePageMaterialDatas[currentLeftPageNumber - 1].leftPagesMaterial : emptyPageMaterial;
        indexNextPage = currentLeftPageNumber - 1;
    }
    
    private void GetNextLeftPageMaterial(out Material mat, out int indexNextPage) //////////////////MODIFIER POUR LE SYSTEME DE PAGE MANQUANTE
    {
        mat = currentLeftPageNumber < pageCount ? doublePageMaterialDatas[currentLeftPageNumber + 1].leftPagesMaterial : emptyPageMaterial;
        indexNextPage = currentLeftPageNumber + 1;
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