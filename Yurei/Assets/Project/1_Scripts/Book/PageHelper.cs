using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PageHelper : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRendererLeft;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRendererRight;

    [SerializeField] private Animator animator;

    public Material GetMaterialLeftMovingPage() => skinnedMeshRendererLeft.material;
    public Material GetMaterialRightMovingPage() => skinnedMeshRendererRight.material;

    public void SetMaterialLeftMovingPage(Material material) => skinnedMeshRendererLeft.material = material;
    public void SetMaterialRightMovingPage(Material material) => skinnedMeshRendererRight.material = material;

    private Action onCompleteAction = null;

    public void PlayPageAnimation(bool goToRight, Action onComplete = null)
    {
        onCompleteAction = onComplete;
        string animationName = goToRight ? "MoveToRight" : "MoveToLeft";
        animator.Play(animationName);
    }

    public void AfterAnimationMoveToRight()
    {
        onCompleteAction?.Invoke();
        onCompleteAction = null;
        
        BookManager.Instance.SetMaterialRightPage(GetMaterialRightMovingPage());
        BookManager.Instance.numberTurningPageToRight--;
        gameObject.SetActive(false);
        Destroy(gameObject, 0.5f);
    }

    public void AfterAnimationMoveToLeft()
    {
        onCompleteAction?.Invoke();
        onCompleteAction = null;
        
        BookManager.Instance.SetMaterialLeftPage(GetMaterialLeftMovingPage());
        BookManager.Instance.numberTurningPageToLeft--;
        gameObject.SetActive(false);
        Destroy(gameObject, 0.5f);
    }
}