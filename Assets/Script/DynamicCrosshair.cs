using UnityEngine;
using UnityEngine.UI;


public class DynamicCrosshair : MonoBehaviour
{
public RectTransform crosshair;
public float expandSize = 60f;
public float normalSize = 40f;
public float speed = 5f;


private float currentSize;


void Update()
{
if (Input.GetMouseButton(0))
{
currentSize = Mathf.Lerp(currentSize, expandSize, Time.deltaTime * speed);
}
else
{
currentSize = Mathf.Lerp(currentSize, normalSize, Time.deltaTime * speed);
}


crosshair.sizeDelta = new Vector2(currentSize, currentSize);
}
}
