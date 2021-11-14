using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/**控制圓形進度條運轉
 * Example:
 * StartCircleProgress();
 * EndCircleProgress();
 */
public class CircleProgress : MonoBehaviour
{
    GameObject progress_circle_gameobject;
    Image progress_circle_current_image_;
    bool can_run_circle_=false;
    private int max_progress_=100;
    private int current_=0;
    private void Awake()
    {
        this.progress_circle_gameobject = GameObject.Find("Progress_Circle");
        this.progress_circle_current_image_ = GameObject.Find("Progress_Circle_Current").GetComponent<Image>();

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (this.can_run_circle_ == true)
        {
            if (this.current_ == this.max_progress_) this.current_ = 0;
            else this.current_++;
            float fill_amount = (float)this.current_ / (float)this.max_progress_;
            this.progress_circle_current_image_.fillAmount = fill_amount;
        }
        else
        {
            this.current_ = 0;
        }
    }
    public void StartCircleProgress()
    {
        this.can_run_circle_ = true;
        this.progress_circle_gameobject.GetComponent<Image>().enabled = true;
        this.progress_circle_current_image_.enabled = true;
    }
    public void EndCircleProgress()
    {
        this.can_run_circle_ = false;
        this.progress_circle_gameobject.GetComponent<Image>().enabled = false;
        this.progress_circle_current_image_.enabled = false;
    }
}
