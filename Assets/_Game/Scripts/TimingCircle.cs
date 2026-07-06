using UnityEngine;

public class TimingCircle : MonoBehaviour
{
    [SerializeField] private Transform outerCircle;
    [SerializeField] private Transform innerCircle;

    private float shrinkTime;
    private float timer = 0f;
    private Vector3 startScale = new Vector3(3f, 3f, 1f);
    private Vector3 targetScale;

    public void OnInit(float timeToShrink)
    {
        shrinkTime = timeToShrink;
        targetScale = innerCircle.localScale;
        outerCircle.localScale = startScale;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        //Noi suy de kich thuoc cua outerCircle tu startScale den targetScale trong shrinkTime giay
        outerCircle.localScale = Vector3.Lerp(startScale, targetScale, timer / shrinkTime);

        //Tu huy sau 0.2s
        if(timer >= shrinkTime + 0.2f)
        {
            Destroy(gameObject);
        }
    }
}
