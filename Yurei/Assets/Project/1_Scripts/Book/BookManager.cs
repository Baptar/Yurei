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
    
    [Header("Player Controller")]
    [SerializeField] private PlayerInputController _input;
    
    [Space(5)]
    [Header("Pages Materials")]
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
    public int currentPageNumber = 0;
    public int pageCount;
    public int numberTurningPageToLeft;
    public int numberTurningPageToRight;
    public bool isLookingBook;
    public bool isCameraMoving;
    
    
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;

    private void Awake() => Instance = this;

    private void Start()
    {
        _input.OnLookBookPressed += HandleLookBook;
        _input.OnExitBookPressed += HandleExitBook;
        _input.OnNextPagePressed += HandleNextPage;
        _input.OnPreviousPagePressed += HandlePreviousPage;
        
        pageCount = doublePageMaterialDatas.Length - 1;
        
        SetMaterialRightPage(pageCount == 0 ? emptyPageMaterial : doublePageMaterialDatas[0].rightPagesMaterial);
        SetMaterialLeftPage(pageCount == 0 ? emptyPageMaterial : doublePageMaterialDatas[0].leftPagesMaterial);
    }

    /// <summary>
    /// Conditions functions
    /// </summary>
    private bool CanLookBook() => !isLookingBook && !isCameraMoving;
    private bool CanExitBook() => isLookingBook && numberTurningPageToLeft <= 0 && numberTurningPageToRight <= 0 && !isCameraMoving;
    private bool CanNextPage() => isLookingBook && numberTurningPageToLeft <= 0 && currentPageNumber < pageCount;
    private bool CanPreviousPage() => isLookingBook && numberTurningPageToRight <= 0 && currentPageNumber > 0;

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
            });
    }
    
    private void ExitBook(Action onComplete = null)
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
    ///     - copy "left" on "right of moving page"
    ///     - change "left" to the next left page 
    ///     - change " left of moving page" to the next right page
    ///     - rotate our page
    ///     - change "right" on "left of moving page"
    ///     - remove moving page
    /// </summary>
    private void NextPage(Action onComplete = null)
    {
        GameObject go = Instantiate(pagePrefab, pageSpawnPoint.position, pageSpawnPoint.rotation);
        if (go.TryGetComponent(out PageHelper page))
        {
            // Get element we'll need
            Material actualLeftMaterial = GetLeftPageMaterial();
            Material nextLeftMaterial = GetNextLeftPageMaterial();
            Material nextRightMaterial = GetNextRightPageMaterial();
            
            currentPageNumber++;
            numberTurningPageToRight++;
            
            // copy "left" on "right of moving page"
            page.SetMaterialRightMovingPage(actualLeftMaterial);
            
            // change "left" to the next left page 
            SetMaterialLeftPage(nextLeftMaterial);
            
            // change " left of moving page" to the next right page
            page.SetMaterialLeftMovingPage(nextRightMaterial);

            page.PlayPageAnimation(true, onComplete);
        }
        
        else Destroy(go);
    }

    /// <summary>
    /// Turn to the previous page
    /// steps :
    ///     - init page position to end
    ///     - copy "right" on "left of moving page"
    ///     - change "right" to the next right page 
    ///     - change " right of moving page" to the next left page
    ///     - rotate our page with reverse animation
    ///     - change "left" on "right of moving page"
    ///     - remove moving page
    /// </summary>
    private void PreviousPage(Action onComplete = null)
    {
        GameObject go = Instantiate(pagePrefab, pageSpawnPoint.position, pageSpawnPoint.rotation);
        if (go.TryGetComponent(out PageHelper page))
        {
            // Get element we'll need
            Material actualRightMaterial = GetRightPageMaterial();
            Material previousLeftMaterial = GetPreviousLeftPageMaterial();
            Material previousRightMaterial = GetPreviousRightPageMaterial();
            
            currentPageNumber--;
            numberTurningPageToLeft++;
            
            // copy "right" on "left of moving page"
            page.SetMaterialLeftMovingPage(actualRightMaterial);
            
            // change "right" to the previous right page 
            SetMaterialRightPage(previousRightMaterial);
            
            // change " right of moving page" to the previous left page
            page.SetMaterialRightMovingPage(previousLeftMaterial);

            page.PlayPageAnimation(false, onComplete);
        }
        
        else Destroy(go);
    }

    /// <summary>
    /// Helpers functions
    /// </summary>
    private Material GetLeftPageMaterial() => bookBase.materials[0];
    private Material GetRightPageMaterial() => bookBase.materials[2];
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

    private Material GetPreviousRightPageMaterial()
    {
        return currentPageNumber > 0 ? doublePageMaterialDatas[currentPageNumber - 1].rightPagesMaterial : emptyPageMaterial;
    }
    
    private Material GetNextRightPageMaterial()
    {
        return currentPageNumber < (doublePageMaterialDatas.Length - 1) ? doublePageMaterialDatas[currentPageNumber + 1].rightPagesMaterial : emptyPageMaterial;
    }

    private Material GetPreviousLeftPageMaterial()
    {
        return currentPageNumber > 0 ? doublePageMaterialDatas[currentPageNumber - 1].leftPagesMaterial : emptyPageMaterial;
    }
    
    private Material GetNextLeftPageMaterial()
    {
        return currentPageNumber < (doublePageMaterialDatas.Length - 1) ? doublePageMaterialDatas[currentPageNumber + 1].leftPagesMaterial : emptyPageMaterial;
    }
    
    public int GetCurrentPageNumber() => currentPageNumber;

    /// <summary>
    /// On Player finish page
    /// </summary>
    public void OnPlayerFinishedPage(bool isRightPage, Action onComplete = null)
    {
        LookBook(() =>
        {
            // after look book 
            if (isRightPage) PreviousPage(onComplete);
            else NextPage(onComplete);
        });
    }
}