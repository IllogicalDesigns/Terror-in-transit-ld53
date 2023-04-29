using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using DG.Tweening;
using UnityEngine.UI;

public class AwarenessUI : MonoBehaviour
{
    [SerializeField] private float bounceMod = 1.2f;

    [Space]
    [SerializeField] private Image fillImage;

    [SerializeField] private AwarenessSystem awareness;

    [Space]
    [SerializeField] private Vector3 offset;

    private TrackedTarget _tracked_target;
    private Slider _slider;
    private GameObject _player;
    private float _old_awareness = 0f;

    // Start is called before the first frame update
    private void Start()
    {
        _slider = gameObject.GetComponentInChildren<Slider>();
        _player = GameObject.Find("Player");

        _tracked_target = awareness.getTrackedGameobject(GameObject.Find("Player"));
        BounceSlider();
    }

    private void BounceSlider()
    {
        //_slider.transform.DOScale(Vector3.one * bounceMod, 0.1f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutBounce);
    }

    private void MarkerMovement()
    {
        //Modified code from Omar Balfaqih https://www.youtube.com/watch?v=oBkfujKPZw8
        // Giving limits to the icon so it sticks on the screen
        // Below calculations witht the assumption that the icon anchor point is in the middle
        // Minimum X position: half of the icon width
        float minX = _slider.image.GetPixelAdjustedRect().width / 2;
        // Maximum X position: screen width - half of the icon width
        float maxX = Screen.width - minX;

        // Minimum Y position: half of the height
        float minY = _slider.image.GetPixelAdjustedRect().height / 2;
        // Maximum Y position: screen height - half of the icon height
        float maxY = Screen.height - minY;

        // Temporary variable to store the converted position from 3D world point to 2D screen point
        Vector2 pos = Camera.main.WorldToScreenPoint(awareness.transform.position + offset);

        //// Check if the target is behind us, to only show the icon once the target is in front
        //if (Vector3.Dot((awareness.transform.position - player.transform.position), player.transform.forward) < 0)
        //{
        //    // Check if the target is on the left side of the screen
        //    if (pos.x < Screen.width / 2)
        //    {
        //        // Place it on the right (Since it's behind the player, it's the opposite)
        //        pos.x = maxX;
        //    }
        //    else
        //    {
        //        // Place it on the left side
        //        pos.x = minX;
        //    }
        //}

        // Limit the X and Y positions
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // Update the marker's position
        _slider.transform.position = pos;
        // Change the meter text to the distance with the meter unit 'm'
        //meter.text = ((int)Vector3.Distance(target.position, transform.position)).ToString() + "m";
    }

    // Update is called once per frame
    private void Update()
    {
        _slider.gameObject.SetActive(_tracked_target != null);

        if (_tracked_target == null)
        {
            _tracked_target = awareness.getTrackedGameobject(_player);
            if (_tracked_target != null)
            {
                BounceSlider();
                return;
            }

            _old_awareness = 0;
        }

        if (_tracked_target == null) return;
        if (fillImage == null) return;

        if (_old_awareness > 0 && _tracked_target.awareness <= 0f)
        {
            _old_awareness = 0;
            _tracked_target = null;
            return;
        }

        MarkerMovement();

        UpdateSliderColorAndBounce();

        _old_awareness = _tracked_target.awareness;
    }

    private void UpdateSliderColorAndBounce()
    {
        if (_tracked_target.awareness < 1f)
        {
            fillImage.color = Color.yellow;
            _slider.value = _tracked_target.awareness;
        }
        else if (_tracked_target.awareness <= 2f)
        {
            fillImage.color = Color.red;
            _slider.value = _tracked_target.awareness - 1;
        }

        if (_old_awareness == 0 && _tracked_target.awareness > 0f)
        {
            BounceSlider();
        }

        if (_old_awareness < 1f && _tracked_target.awareness >= 1f)
        {
            BounceSlider();
        }

        if (_old_awareness < 2f && _tracked_target.awareness >= 2f)
        {
            BounceSlider();
        }
    }
}
