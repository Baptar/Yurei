using UnityEngine;

//Ce script gère un RTPC de panning en fonction de la position de l'objet dans l'écran.
//Objet à gauche de l'écran = spatialisé à gauche
[RequireComponent(typeof(AkGameObj))]
public class AudioBookSpatialisation : MonoBehaviour
{
    [Header("Sound")]
    public AK.Wwise.Event soundEvent;
    public bool playOnStart = true;

    [Header("RTPC")]
    private AK.Wwise.RTPC panRTPC;
    float lastPan;

    void Start()
    {
        if (playOnStart && soundEvent != null)
            soundEvent.Post(gameObject);
    }

    public void PlaySound()
    {
        if (soundEvent != null)
        AudioServices.Events.PostEvent(soundEvent, gameObject);
    }

    //Update pan <-> position de l'objet
    void Update()
    {
        if (Camera.main == null)
        {
        Debug.LogWarning("Add tag 'MainCamera' to the camera filming the book.");
        return;
        }

        //Position de l'objet entre 0 et 1, convertie entre -100 et +100
        var vp = Camera.main.WorldToViewportPoint(transform.position);
        var pan = Mathf.Clamp((vp.x - 0.5f) * 200f, -100f, 100f);

        //Changement de pan minime : pas d'update pour éviter le spam
        if (Mathf.Abs(pan - lastPan) < 0.1f) return;

        AudioServices.RTPC.SetRTPCValue("Book_Panning", pan, gameObject);

        lastPan = pan;
    }
}