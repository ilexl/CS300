using System.Collections.Generic;
using UnityEngine;

public class TankSelection : MonoBehaviour
{
    [SerializeField] List<RectTransform> TankList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RecalculateTankListWidth();
    }

    void RecalculateTankListWidth()
    {
        foreach(RectTransform holder in TankList)
        {
            int childCount = holder.transform.childCount;
            int width = (childCount * 240) + (9 * (childCount + 1)); // width of each child + border.
            if(width < Screen.width)
            {
                width = Screen.width;
            }
            holder.sizeDelta = new Vector2(width, holder.rect.height);  
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
