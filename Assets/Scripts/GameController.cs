using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public struct Gamevars
{
    public static bool istailoring=false;
    public static bool tailoring=false;
    public static bool textureisable=false;
    public static int imagewidth = 0;
    public static int imageheight = 0;
    public static float size=1;
    public static float[,] xy = new float[2, 2];
    public static Vector2 mospos=new Vector2(0,0);
    public static Vector2 mospos2 = new Vector2(0, 0);
}
/// <summary>
/// 图片处理器主控制逻辑类
/// </summary>
public class GameController : MonoBehaviour {
    private float xx = 0;
    private float yy = 0;
    public Text size1;
    public Camera ca;
    public RawImage image;
    [Range(.1f, 1f)]
    public float smooth=.2f;
    private Vector2 vectormtoo;
    private float fire = 0;
    private LineRenderer linerd;
    private Vector2 mospos=new Vector2(0,0);
    private Vector2 mospos2 = new Vector2(0, 0);
    // Use this for initialization
    void Start () {
        linerd = this.GetComponent<LineRenderer>();
        linerd.SetVertexCount(5);
        linerd.SetWidth(0, 0);
	}
	public void tailor()
    {
        Gamevars.istailoring = true;
    }
	// Update is called once per frame
	void Update () {

        if (Gamevars.textureisable)
        {
            if (Gamevars.istailoring && Gamevars.tailoring&&Input.mousePosition.x>900&&Input.GetMouseButtonUp(0))
            {
                Gamevars.istailoring = false;
                Gamevars.tailoring = false;
                linerd.SetWidth(0, 0);
            }
            if (Input.GetAxis("Mouse ScrollWheel") != 0 &&!Gamevars.istailoring&& Input.mousePosition.x <= 900)
            {
                Vector2 mid = new Vector2(Gamevars.imagewidth, Gamevars.imageheight) * Gamevars.size;
                Gamevars.size += Input.GetAxis("Mouse ScrollWheel") * smooth;
                Vector2 sizedelta= new Vector2(Gamevars.imagewidth, Gamevars.imageheight) * Gamevars.size;
                mid = sizedelta - mid;//用来记录缩放量
                Vector2 originpos = new Vector2(image.transform.position.x, image.transform.position.y - sizedelta.y);
                Vector2 mousepos = Input.mousePosition;
                Vector2 mouseposinimage = mousepos - originpos;
                xx = mouseposinimage.x / sizedelta.x;
                yy = mouseposinimage.y / sizedelta.y;
                image.GetComponent<RectTransform>().sizeDelta = sizedelta;
                size1.text = ((int)(Gamevars.size * 100)).ToString() + "%";
                if (!(xx < 0 || xx > 1 || yy < 0 ||yy > 1))
                {
                    //Debug.Log(xx);
                    //Debug.Log(yy);
                   image.GetComponent<RectTransform>().Translate(new Vector3(-xx * mid.x, (1-yy)* mid.y, 0));
                }
            }
            if (Input.GetAxis("Fire1") != 0 && !Gamevars.istailoring && Input.mousePosition.x<=900)
            {
                
                Vector2 mousepos = Input.mousePosition;
                if (fire == 0) {
                    vectormtoo = mousepos - (Vector2)image.transform.position;
                    fire = 1;
                }
                image.transform.position = mousepos - vectormtoo;
            }
            if (fire==1&&Input.GetAxis("Fire1") == 0)
            {
                fire = 0;
            }
            if (Gamevars.istailoring&& Input.mousePosition.x <= 900)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    linerd.SetPosition(0, mospos = Input.mousePosition);
                    Gamevars.tailoring = true;
                    linerd.SetWidth(1.6f, 1.6f);
                }
                linerd.SetPosition(1, new Vector2(Input.mousePosition.x, mospos.y));
                linerd.SetPosition(2, new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                linerd.SetPosition(3, new Vector2(mospos.x, Input.mousePosition.y));
                linerd.SetPosition(4, mospos);
                if (Gamevars.tailoring && Input.GetMouseButtonUp(0))
                {
                    if (true)
                    {
                        Vector2 sizedelta = new Vector2(Gamevars.imagewidth, Gamevars.imageheight) * Gamevars.size;
                        Vector2 originpos = new Vector2(image.transform.position.x, image.transform.position.y - sizedelta.y);
                        Gamevars.xy[0, 0] = (mospos.x - originpos.x)/sizedelta.x;
                        Gamevars.xy[0, 1] = (mospos.y - originpos.y)/sizedelta.y;
                        Gamevars.xy[1, 0] = (Input.mousePosition.x - originpos.x) / sizedelta.x;
                        Gamevars.xy[1, 1] = (Input.mousePosition.y - originpos.y) / sizedelta.y;
                        Gamevars.mospos = mospos;
                        Gamevars.mospos2 = Input.mousePosition;
                        for (int i = 0; i < 2; i++)//大范围控制
                        {
                            for (int k = 0; k < 2; k++)
                            {
                                if (Gamevars.xy[i, k] < 0)
                                {
                                    Gamevars.xy[i, k] = 0;
                                }
                                else if (Gamevars.xy[i, k] > 1)
                                {
                                    Gamevars.xy[i, k] = 1;
                                }
                            }
                        }
                        //Debug.Log(xy[0, 0].ToString() + "   " + xy[0, 1].ToString());
                        this.GetComponent<Transmit>().tailor();
                        if (Gamevars.xy[1, 0] > Gamevars.xy[0, 0] && Gamevars.xy[1, 1] < Gamevars.xy[0, 1])//位置保持
                        {
                            image.GetComponent<RectTransform>().position = Gamevars.mospos;
                        }
                        else if (Gamevars.xy[1, 0] > Gamevars.xy[0, 0] && Gamevars.xy[1, 1] > Gamevars.xy[0, 1])
                        {
                            image.GetComponent<RectTransform>().position = new Vector2(mospos.x, Gamevars.mospos2.y);
                        }
                        else if (Gamevars.xy[1, 1] > Gamevars.xy[0, 1] && Gamevars.xy[1, 0] < Gamevars.xy[0, 0])
                        {
                            image.GetComponent<RectTransform>().position = Gamevars.mospos2; 
                        }
                        else if(Gamevars.xy[0, 1] > Gamevars.xy[1, 1] && Gamevars.xy[1, 0] < Gamevars.xy[0, 0])
                        {
                            image.GetComponent<RectTransform>().position = new Vector2(Gamevars.mospos2.x, mospos.y);
                        }
                    }
                    Gamevars.istailoring = false;
                    Gamevars.tailoring = false;
                    linerd.SetWidth(0, 0);

                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Gamevars.istailoring = false;
                Gamevars.tailoring = false;
                linerd.SetWidth(0, 0);
            }

        }

	}

}
