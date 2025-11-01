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

    public void PlayPageAnimation(bool goToRight)
    {
        string animationName = goToRight ? "MoveToRight" : "MoveToLeft";
        animator.Play(animationName);
    }

    public void AfterAnimationMoveToRight()
    {
        Debug.Log("AfterANimationMoveToRight");
        BookManager.Instance.SetMaterialRightPage(GetMaterialLeftMovingPage());
        BookManager.Instance.numberTurningPageToRight--;
        Destroy(gameObject, 0.5f);
    }
    
    public void AfterAnimationMoveToLeft()
    {
        Debug.Log("AfterANimationMoveToLeft");
        BookManager.Instance.SetMaterialLeftPage(GetMaterialRightMovingPage());
        BookManager.Instance.numberTurningPageToLeft--;
        Destroy(gameObject, 0.5f);
    }
}
