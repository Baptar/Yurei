using UnityEngine;

public class BookManager : MonoBehaviour
{
    [Header("DEBUG")]
    public int currentPageNumber = 0;

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
    private void NextPage()
    {
        
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
    private void PreviousPage()
    {
        
    }
}
