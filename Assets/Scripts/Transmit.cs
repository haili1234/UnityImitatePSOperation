using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Windows.Forms;
using System.IO;
using UnityEngine.SceneManagement;

/// <summary>
/// 转换类（中间层）
/// </summary>
public class Transmit : PostEffectsBase 
{
    List<RenderTexture> RTs = new List<RenderTexture>();//这两个泛型是用来回收RT占用内存的
    List<RenderTexture> RTpic = new List<RenderTexture>();
    public Text size1;
    public Text resolution;
    private bool overturnX=false;
    private bool overturnY=false;//翻转状态
    private bool clockw = false;
    private bool anclockw = false;//旋转状态
    public UnityEngine.UI.RawImage image;
    private Texture2D texture;
    public Shader processshader;//亮度对比度饱和度着色器
    private Material processmat;
    private Material material
    {
        get
        {
            processmat = CheckShaderAndCreateMaterial(processshader, processmat);
            return processmat;
        }
    }
    public Shader overturnshader;//翻转着色器
    private Material ovmat;
    private Material overturnmaterial
    {
        get
        {
            ovmat = CheckShaderAndCreateMaterial(overturnshader, ovmat);
            return ovmat;
        }
    }
    public Shader rotateshader;//旋转着色器
    private Material romat;
    private Material rotatematerial
    {
        get
        {
            romat = CheckShaderAndCreateMaterial(rotateshader, romat);
            return romat;
        }
    }
    public Shader tailorshader;//gamma矫正着色器
    private Material taimat;
    private Material tailormaterial
    {
        get
        {
            taimat = CheckShaderAndCreateMaterial(tailorshader, taimat);
            return taimat;
        }
    }
    //public Shader grayshader;
    //private Material graymat;
    //private Material graymaterial
    //{
    //    get
    //    {
    //        graymat = CheckShaderAndCreateMaterial(grayshader, graymat);
    //        return graymat;
    //    }
    //}
    private Texture2D Viewtexture;//预览图
    private string path;
    [Range(0.0f, 3.0f)]
    public float brightness = 1.0f;
    [Range(0.0f, 3.0f)]
    public float saturation = 1.0f;
    [Range(0.0f, 3.0f)]
    public float contrast = 1.0f;
    public Slider slider1;
    private float slider1lastvalue=1;
    public void BscChange1()
    {
        brightness = slider1.value;
        if (Mathf.Abs(brightness - slider1lastvalue) >= 0.1)//降低精度，优化运行速度，减少draw call
        {
            slider1lastvalue = Mathf.FloorToInt(brightness * 10) / 10;
            updateBSC();
        }
    }
    public Slider slider2;
    private float slider2lastvalue = 1;
    public void BscChange2()
    {
        saturation = slider2.value;
        if (Mathf.Abs(saturation - slider2lastvalue) >= 0.1f)
        {
            slider2lastvalue = Mathf.FloorToInt(saturation * 10) / 10;
            updateBSC();
        }
    }
    public Slider slider3;
    private float slider3lastvalue = 1;
    public void BscChange3()
    {
        contrast = slider3.value;
        if (Mathf.Abs(contrast - slider3lastvalue) >= 0.1f)
        {
            slider3lastvalue = Mathf.FloorToInt(contrast * 10) / 10;
            updateBSC();
        }
    }
    public Slider slider4;
    private float gammavalue=1;
    //public UnityEngine.UI.Text gammatext;
    public void BscChange4()
    {
        //gammatext.text = slider4.value.ToString();
        gammavalue = slider4.value;
        updateBSC();
    }
    private bool graybool = false;
    public Toggle toggle;
    public void graychange()
    {
        graybool = toggle.isOn;
        updateBSC();
        
    }
    public void tailor()
    {
        dotailor();
    }
    public UnityEngine.UI.Button selectfile;
    public void Selectpic()//文件选择
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.Multiselect = false;//只能选择一个文件
        dialog.Title = "请选择图片";
        dialog.Filter = "图像文件(*.bmp;*.jpg;*.jpeg;*.png)|*.bmp;*.jpg;*.jpeg;*.png";
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            path = dialog.FileName;
            selectfile.gameObject.SetActive(false);
            StartCoroutine(Load());
            
        }
    }
    public void horizon()
    {
        overturnX = true;
        Overturn();
    }
    public void vertical()
    {
        overturnY = true;
        Overturn();
    }
    public void clockwise()
    {
        clockw = true;
        rotate();
    }
    public void anticlockwise()
    {
        anclockw = true;
        rotate();
    }
    public void fileclose()
    {
        if (Gamevars.textureisable)
        {
            Gamevars.textureisable = false;
            selectfile.gameObject.SetActive(true);
            Gamevars.size = 1;
            size1.text = ((int)(Gamevars.size * 100)).ToString() + "%";
            image.color = new Color32(255, 255, 255, 0);
            image.transform.position = new Vector3(0, 720, 0);
            slider1.value = 1;
            slider2.value = 1;
            slider3.value = 1;
            slider4.value = 1;
            resolution.text = "0 x 0";
            SceneManager.LoadScene(0);//清内存很方便
        }
    }
    public void savefile()
    {
        if (Gamevars.textureisable)
        {
            
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "请选择保存位置";
            dialog.Filter = "图像文件(*.jpg)|*.jpg";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Viewtexture = new Texture2D(Gamevars.imagewidth, Gamevars.imageheight, TextureFormat.ARGB32, false);
                RenderTexture.active = RTs[0];
                Viewtexture.ReadPixels(new Rect(0, 0, Gamevars.imagewidth, Gamevars.imageheight), 0, 0);
                Viewtexture.Apply();
                RenderTexture.active = null;
                byte[] bytes = Viewtexture.EncodeToJPG();
                File.WriteAllBytes(dialog.FileName, bytes);
            }

        }
    }
    public void exitgame()
    {

            UnityEngine.Application.Quit();
       
    }
    IEnumerator Load()//图片读取携程
    {
        WWW www = new WWW(path);
        yield return www;
        texture = www.texture;
        image.GetComponent<RectTransform>().sizeDelta= new Vector2(texture.width, texture.height);
        //image.GetComponent<Image>().material.SetTexture("_MainTex", texture);
        //Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        //image.sprite = sprite;
        image.GetComponent<RawImage>().texture = texture;
        Gamevars.textureisable = true;
        Gamevars.imagewidth = texture.width;
        Gamevars.imageheight = texture.height;
        resolution.text = Gamevars.imagewidth.ToString() + " x " + Gamevars.imageheight.ToString();
        image.color = new Color32(255, 255, 255, 255);
        RenderTexture Disttexture = RenderTexture.GetTemporary(texture.width, texture.height, 0);
        Graphics.Blit(texture, Disttexture, material);
        RTpic.Insert(0,Disttexture);
    }
    //private RenderTexture Rt;
    private void updateBSC()//BSCshader更新
    {
        if (Gamevars.textureisable)
        {
            material.SetFloat("_Brightness", brightness);
            material.SetFloat("_Saturation", saturation);
            material.SetFloat("_Contrast", contrast);
            material.SetInt("_Gray", graybool?1:0);
            //material.SetInt("_Usegamma", toggle2.isOn ? 1 : 0);
            material.SetFloat("_Gamma", gammavalue);
            for (int i = 0; i < RTs.Count; i++)
            {
                RenderTexture.ReleaseTemporary(RTs[i]);
                RTs.Clear();
            }
            RenderTexture Disttexture = RenderTexture.GetTemporary(RTpic[0].width, RTpic[0].height, 0);
            //弃用，大量使用RT时，内存爆炸RenderTexture Disttexture = new RenderTexture(texture.width, texture.height, 0);
            //弃用，DestroyImmediate(Disttexture);
            Graphics.Blit(RTpic[0], Disttexture, material);
            RTs.Add(Disttexture);
            //Debug.Log(Disttexture == null);
            int width = Disttexture.width;
            int height = Disttexture.height;
            //Viewtexture = null;
            //Viewtexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            //RenderTexture.active = Disttexture;
            //Viewtexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            //Viewtexture.Apply();
            //RenderTexture.active = null;
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);//因为可能image已经缩放了
            //弃用，Rt转Sprite太慢//sprite = null;
            //sprite = Sprite.Create(Viewtexture, new Rect(0, 0, Viewtexture.width, Viewtexture.height), new Vector2(0.5f, 0.5f));//因为居中显示所以.5f
            //Debug.Log(System.DateTime.Now.Second.ToString() + "  " + System.DateTime.Now.Millisecond.ToString());//消耗检测
            //image.sprite = sprite;
            image.GetComponent<RawImage>().texture = Disttexture;
            
            Resources.UnloadUnusedAssets();
            Refresh();
        }
    }
    private void Overturn()//翻转shader更新
    {
        if (Gamevars.textureisable)
        {
            overturnmaterial.SetInt("_Horizon", overturnX ? 1 : 0);
            overturnmaterial.SetInt("_Vertical", overturnY ? 1 : 0);
            RenderTexture Disttexture = RenderTexture.GetTemporary(RTpic[0].width, RTpic[0].height, 0);
            Graphics.Blit(RTpic[0], Disttexture, overturnmaterial);
            RenderTexture.ReleaseTemporary(RTpic[0]);
            RTpic.Insert(0,Disttexture);
            int width = Disttexture.width;
            int height = Disttexture.height;
            //弃用，RT转换为textur也慢texture = null;
            //texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            //RenderTexture.active = Disttexture;
            //texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            //texture.Apply();
            //RenderTexture.active = null;
           
            //texture = texture;//因为旋转之后，仍需要其他shader参与
            updateBSC();
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            //sprite = null;
            //sprite = Sprite.Create(Viewtexture, new Rect(0, 0, Viewtexture.width, Viewtexture.height), new Vector2(0.5f, 0.5f));
            //image.sprite = sprite;
            image.GetComponent<RawImage>().texture = RTs[0];
            Resources.UnloadUnusedAssets();
            Refresh();
        }
        overturnX = false;
        overturnY = false;
    }
    private void rotate()//旋转shader更新
    {
        if (Gamevars.textureisable)
        {
            rotatematerial.SetInt("_Clockwise", clockw ? 1 : 0);
            rotatematerial.SetInt("_AnuiClockwise", anclockw? 1:0);
            RenderTexture Disttexture =RenderTexture.GetTemporary(RTpic[0].height, RTpic[0].width, 0);//因为旋转之后宽高对调
            Graphics.Blit(RTpic[0], Disttexture, rotatematerial);
            RenderTexture.ReleaseTemporary(RTpic[0]);
            RTpic.Insert(0, Disttexture);
            int width = Disttexture.width;
            int height = Disttexture.height;
            resolution.text=width.ToString()+" x "+height.ToString();
            Gamevars.imagewidth = width;
            Gamevars.imageheight = height;//更新控制结构体
            //texture = null;
            //texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            //RenderTexture.active = Disttexture;
            //texture.ReadPixels(new Rect(0, 0, width, height),0, 0);
            //texture.Apply();
            //RenderTexture.active = null;
            //texture = Viewtexture;
            updateBSC();
            Vector3 centrepos = image.transform.position + new Vector3(height / 2, -width / 2, 0)*Gamevars.size;//
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(width,height);
            image.transform.position = centrepos - new Vector3(width / 2, -height / 2, 0)*Gamevars.size;//这两行是为了让旋转不反人类
            //sprite = null;
            //sprite = Sprite.Create(Viewtexture, new Rect(0, 0,width, height), new Vector2(0.5f, 0.5f));
            //image.sprite = sprite;
            image.GetComponent<RawImage>().texture = RTs[0];
            Resources.UnloadUnusedAssets();
            Refresh();
        }
        clockw = false;
        anclockw = false;
    }
    private void dotailor()//裁剪
    {
        tailormaterial.SetFloat("x1",Gamevars.xy[0, 0]);
        tailormaterial.SetFloat("y1", Gamevars.xy[0, 1]);
        tailormaterial.SetFloat("x2", Gamevars.xy[1, 0]);
        tailormaterial.SetFloat("y2", Gamevars.xy[1, 1]);
        RenderTexture Disttexture = RenderTexture.GetTemporary((int)(RTpic[0].width*(Mathf.Abs(Gamevars.xy[0,0]-Gamevars.xy[1,0]))), (int)(RTpic[0].height * (Mathf.Abs(Gamevars.xy[0, 1] - Gamevars.xy[1, 1]))), 0);
        int width = Disttexture.width;
        int height = Disttexture.height;
        resolution.text = width.ToString() + " x " + height.ToString();
        Gamevars.imageheight = height;
        Gamevars.imagewidth = width;
        tailormaterial.SetFloat("Scalex", (float)RTpic[0].width/ width );
        tailormaterial.SetFloat("Scaley", (float)RTpic[0].height/ height);//这个float很重要，因为都是int会导致精度丢失
        Graphics.Blit(RTpic[0], Disttexture, tailormaterial);
        RenderTexture.ReleaseTemporary(RTpic[0]);//
        RTpic.Insert(0, Disttexture);
       // Debug.Log(width.ToString() + "   " + height.ToString());
        updateBSC();
        Vector2 sized = image.GetComponent<RectTransform>().sizeDelta;
        image.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        image.GetComponent<RawImage>().texture = RTs[0];
        Resources.UnloadUnusedAssets();
        Refresh();
    }
    private void Refresh()
    {
        image.GetComponent<RectTransform>().sizeDelta = new Vector2(Gamevars.imagewidth, Gamevars.imageheight) * Gamevars.size;
    }
    //private void awake()
    //{
    //    //slidergamma.enabled = toggle2.ison;
    //}
}
