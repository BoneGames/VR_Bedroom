using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using NaughtyAttributes;

public class Driver : MonoBehaviour
{
    public class Vector4Class
    {
        public int x, y, z, w;

        public Vector4Class(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }

    [Serializable]
    public struct Surface
    {
        // texture on BG Material
        public Texture2D bg;
        // Texture on Render Layer
        public Texture2D renderLayer;
        // replacement texture for bg layer
        public Color32[] baseRenderLayerArray;
        public Vector2Int gridLoc;
        public Material renderMat;
        public Vector4Class fruitLoc;

        public Surface(Texture2D bg, Texture2D renderLayer, Color32[] baseRenderLayer, Vector2Int id, Material renderMat, Vector4Class fruitLoc)
        {
            this.bg = bg;
            this.renderLayer = renderLayer;
            this.baseRenderLayerArray = baseRenderLayer;
            this.gridLoc = id;
            this.renderMat = renderMat;
            this.fruitLoc = fruitLoc;
        }
    }
    public struct CanvasPlacement
    {
        public Surface surface;
        public Vector2Int offset;
        public Texture2D petPortion;

        public CanvasPlacement(Surface surface, Vector2Int offset, Texture2D petPortion)
        {
            this.surface = surface;
            this.offset = offset;
            this.petPortion = petPortion;
        }
    }

    // Setup
    public int layoutBuffer;
    public int squareSize;
    public Color bgCol, renderLayerCol;
    public Vector2Int res;
    public GameObject planePrefab;
    [Range(0.01f, 1f)]
    public float planeScale;
    public Transform targetSurface;

    //
    public Texture2D pet;
    public bool animated;
    public int moveSpeed;
    float petAspectRatio;
    [ShowIf(nameof(animated))]
    public float spriteChangeCounter;
    [ShowIf(nameof(animated))]
    public float spriteChangeDelay;
    public int growthRate;
    public bool showTrail;

    [ShowIf(nameof(animated))]
    public List<Texture2D> petFrames = new List<Texture2D>();
    [ShowIf(nameof(animated))]
    public Vector2Int cellSlicePartitions;
    int petFrameIndex;
    Vector2Int spriteSliceOffset;


    Color32[] bgColArray, renderColArray;
    List<List<Surface>> surfaceGrid = new List<List<Surface>>();
    public Vector2Int baseSurfaceLocation;
    public Vector2Int offset;

    GameObject gridParent;
    public Texture2D[] numbers;
    public Texture2D[] food;


    void Start()
    {
        if (gridParent == null)
        {
            InitPlaneLayout();
        }
        SetBaseSurfaceColors();

        SlicePetSpriteSheet();

        // setup collections and texture walls
        Make2DMatsList();

        // init start point
        baseSurfaceLocation = GetRandomSurfaceLocation();

        offset = GetRandomOffset(pet);
        // draw pet
        ConfigurePetDivisions();
    }

    [Button]
    void InitPlaneLayout()
    {
        Debug.Log("IPL");
        gridParent = new GameObject();
        gridParent.transform.position = targetSurface.position;
        //gridParent.transform.localScale = targetSurface.localScale;
        gridParent.transform.rotation = targetSurface.rotation;
        //gridParent.transform.forward = targetSurface.right;
        //gridParent.transform.up = targetSurface.up;
        //gridParent.transform.forward = targetSurface.right * -1;
        //gridParent.transform.position = Vector3.zero;
        gridParent.name = "Grid";
        gridParent.tag = "Grid";

        float xOffset = (planePrefab.transform.localScale.x * 10 * planeScale);
        float zOffset = (planePrefab.transform.localScale.z * 10 * planeScale);

        //Quaternion parentRot = gridParent.transform.rotation;

        float xCentreOffset = ((squareSize - 1) * xOffset + (layoutBuffer * (squareSize - 1) * planeScale)) * 0.5f;
        float zCentreOffset = ((squareSize - 1) * zOffset + (layoutBuffer * (squareSize - 1) * planeScale)) * 0.5f;

        Vector3 sizeOffset = new Vector3(xCentreOffset, zCentreOffset);
        gridParent.transform.position -= sizeOffset;
        for (int i = 0; i < squareSize; i++)
        {
            for (int j = 0; j < squareSize; j++)
            {
                Vector3 position = new Vector3((i * xOffset) + (layoutBuffer * i * planeScale), 0, (j * zOffset) + (layoutBuffer * j * planeScale));
                GameObject go = Instantiate(planePrefab, gridParent.transform);
                go.transform.localPosition = position;
                go.transform.localScale *= planeScale;
            }
        }


    }

    Vector2Int GetRandomSurfaceLocation()
    {
        // init value
        Vector2Int rSurfaceLocation = new Vector2Int(UnityEngine.Random.Range(0, surfaceGrid.Count), UnityEngine.Random.Range(0, surfaceGrid[0].Count));
        // check original from pet position
        while (rSurfaceLocation == baseSurfaceLocation)
        {
            rSurfaceLocation = new Vector2Int(UnityEngine.Random.Range(0, surfaceGrid.Count), UnityEngine.Random.Range(0, surfaceGrid[0].Count));
        }
        // return value
        return rSurfaceLocation;
    }

    void SetBaseSurfaceColors()
    {
        // create texture containers
        Texture2D baseBgSurface = new Texture2D(res.x, res.y);
        Texture2D baseRenderSurface = new Texture2D(res.x, res.y);
        // write colors
        for (int i = 0; i < baseBgSurface.width; i++)
        {
            for (int j = -baseBgSurface.height; j < 0; j++)
            {
                baseBgSurface.SetPixel(i, j, bgCol);
                baseRenderSurface.SetPixel(i, j, renderLayerCol);
            }
        }
        // init wall color Array
        bgColArray = baseBgSurface.GetPixels32();
        renderColArray = baseRenderSurface.GetPixels32();
    }

    void SlicePetSpriteSheet()
    {
        if (animated)
        {
            // set initial offset counter
            int _x = cellSlicePartitions.x != 0 ? (int)pet.width / cellSlicePartitions.x : 0;
            int _y = cellSlicePartitions.y != 0 ? (int)pet.height / cellSlicePartitions.y : 0;


            spriteSliceOffset = new Vector2Int(_x, _y);
            // init first offset
            Vector2Int _offset = Vector2Int.zero;
            for (int i = 0; i < (cellSlicePartitions.x + 1) * (cellSlicePartitions.y + 1); i++)
            {
                // create texture container
                Texture2D petShot = new Texture2D(spriteSliceOffset.x, pet.height);
                for (int j = 0; j < petShot.width; j++)
                {
                    for (int k = 0; k < petShot.height; k++)
                    {
                        // write texture
                        petShot.SetPixel(j, k, pet.GetPixel(_offset.x + j, _offset.y + k));
                    }
                }
                // GPU - CPU
                petShot.Apply();
                // add to anim list
                petFrames.Add(petShot);
                // increment offset for slicing
                _offset = new Vector2Int(_offset.x + spriteSliceOffset.x, offset.y + spriteSliceOffset.y);
            }
        }
        // init pet sprite
        if (animated)
            pet = petFrames[0];
        // set aspect ratio
        petAspectRatio = pet.width / pet.height;
    }

    Vector2Int GetConnectingSurfaceLocation(Vector2 dir)
    {
        // NOTE - DOES THE DIAGONAL APPROACH ACTUALLY DO ALL CHANGES? - test later


        // if diagonal change
        if (dir.x != 0 && dir.y != 0)
        {
            if (dir.x == 1 && dir.y == 1)
            {
                int xIndex = 0;
                int yIndex = 0;

                // x below max
                if (surfaceGrid.Count > baseSurfaceLocation.x + dir.x)
                {
                    // x above min
                    if (baseSurfaceLocation.x + dir.x > 0)
                    {
                        xIndex = baseSurfaceLocation.x + (int)dir.x;
                    }
                    // x below min
                    else
                    {
                        xIndex = surfaceGrid.Count - 1;
                    }
                }
                else // x above max
                {
                    xIndex = 0;
                }

                // y below max
                if (surfaceGrid[baseSurfaceLocation.x].Count > baseSurfaceLocation.y + dir.y)
                {
                    // above min
                    if (baseSurfaceLocation.y + dir.y > 0)
                    {
                        yIndex = baseSurfaceLocation.y + (int)dir.y;
                    }
                    // below min
                    else
                    {
                        yIndex = surfaceGrid[baseSurfaceLocation.x].Count - 1;
                    }
                }
                // above max
                else
                {
                    yIndex = 0;
                }

                return new Vector2Int(xIndex, yIndex);
            }
            else
            {
                Debug.LogError("cannot get connecting surface for diagonal direction: " + dir);
                return new Vector2Int();
            }
        }


        // if x direction change (which list)
        if (dir.x != 0)
        {
            // below max
            if (surfaceGrid.Count > baseSurfaceLocation.x + dir.x)
            {
                // above min
                if (baseSurfaceLocation.x + dir.x >= 0)
                {
                    // return +1 x
                    //return wallGrid[currentCanvas.x + (int)dir.x][currentCanvas.y];
                    return new Vector2Int(baseSurfaceLocation.x + (int)dir.x, baseSurfaceLocation.y);
                }
                // below min
                else
                {
                    // return max - other end of X
                    //return wallGrid[wallGrid.Count - 1][currentCanvas.y];
                    return new Vector2Int(surfaceGrid.Count - 1, baseSurfaceLocation.y);
                }
            }
            else // above max
            {
                // retunr min - other end of X
                return new Vector2Int(0, baseSurfaceLocation.y);
                //return wallGrid[0][currentCanvas.y];
            }
        }
        // if Y direction change
        else
        {
            // below max
            if (surfaceGrid[baseSurfaceLocation.x].Count > baseSurfaceLocation.y + dir.y)
            {
                // above min
                if (baseSurfaceLocation.y + dir.y >= 0)
                {
                    // +1 Y
                    // return wallGrid[currentCanvas.x][currentCanvas.y + (int)dir.y];
                    return new Vector2Int(baseSurfaceLocation.x, baseSurfaceLocation.y + (int)dir.y);
                }
                // below min
                else
                {
                    // return max - other end of y
                    // return wallGrid[currentCanvas.x][wallGrid[currentCanvas.x].Count - 1];
                    return new Vector2Int(baseSurfaceLocation.x, surfaceGrid[baseSurfaceLocation.x].Count - 1);
                }
            }
            // above max
            else
            {
                // return wallGrid[currentCanvas.x][0];
                return new Vector2Int(baseSurfaceLocation.x, 0);
            }
        }
    }

    void SetFruitLocation(Surface surface, Vector4Class newLocation)
    {
        Vector2Int loc = surface.gridLoc;
        Vector4Class oldLocation = surfaceGrid[loc.x][loc.y].fruitLoc;
        surfaceGrid[loc.x][loc.y] = new Surface(surface.bg, surface.renderLayer, surface.baseRenderLayerArray, surface.gridLoc, surface.renderMat, newLocation);
        Debug.Log("ID: " + surface.gridLoc + ", old location: " + oldLocation + " Set to: " + surfaceGrid[loc.x][loc.y].fruitLoc);
    }

    void SetBaseRenderLayerArray(Surface surface, Color32[] newBaseRenderColArray)
    {
        Vector2Int loc = surface.gridLoc;
        surfaceGrid[loc.x][loc.y] = new Surface(surface.bg, surface.renderLayer, newBaseRenderColArray, surface.gridLoc, surface.renderMat, surface.fruitLoc);
    }

    [Button]
    public void InsertFruit()
    {
        // grab random surface
        Vector2Int randLoc = GetRandomSurfaceLocation();
        Surface randomSurface = GetRenderSurface(randLoc);
        // get fruit tex
        Texture2D fruit = food[UnityEngine.Random.Range(0, food.Length - 1)];
        // create fruit offset
        Vector2Int fruitOffset = GetRandomOffset(fruit);
        // create fruit location data
        Vector4Class fruitLocation = new Vector4Class(fruitOffset.x, fruitOffset.y, fruit.width, fruit.height);
        // write fruit onto render layer on surface
        WritePixels(randomSurface.renderLayer, fruit, fruitOffset);
        // reapply texture to material
        randomSurface.renderMat.mainTexture = randomSurface.renderLayer;
        // reapply surface struct to grid
        surfaceGrid[randLoc.x][randLoc.y] = new Surface(randomSurface.bg, randomSurface.renderLayer, randomSurface.renderLayer.GetPixels32(), randomSurface.gridLoc, randomSurface.renderMat, fruitLocation);

        Debug.Log("put fruit on " + randomSurface.gridLoc);
    }

    Texture2D GetTransTex()
    {
        Texture2D output = new Texture2D(res.x, res.y);
        output.SetPixels32(renderColArray);
        output.Apply();
        return output;
    }

    Texture2D GetNumberedBgTex(int number)
    {
        // get number texture
        Texture2D num = numbers[Mathf.Clamp(number, 0, numbers.Length - 1)];
        // create new bg texture
        Texture2D numberedBG = new Texture2D(res.x, res.y);
        // write bg col to bg
        numberedBG.SetPixels32(bgColArray);
        // write number on top of bg col
        numberedBG.SetPixels32(0, 0, num.width, num.height, num.GetPixels32());
        // apply changes
        numberedBG.Apply();

        return numberedBG;
    }
    Surface InitSurface(int index, int xGrid, int yGrid, GameObject item)
    {
        // get numbered BG texture
        Texture2D numberedBG = GetNumberedBgTex(index);
        // apply it to be material
        item.GetComponent<Renderer>().materials[0].mainTexture = numberedBG;
        // create new render layer tex
        Texture2D newRenderTex = GetTransTex();
        // get ref to material
        Material render_Mat = item.transform.GetChild(0).GetComponent<Renderer>().materials[0];
        // apply texture to render layer material
        render_Mat.mainTexture = newRenderTex;
        // create struct
        Surface s = new Surface(numberedBG, newRenderTex, renderColArray, new Vector2Int(xGrid, yGrid), render_Mat, new Vector4Class(0, 0, 0, 0));

        return s;
    }
    void Make2DMatsList()
    {
        GameObject[] surfaceBGs = GameObject.FindGameObjectsWithTag("surface");

        // randomize configuration
        System.Random rnd = new System.Random();
        surfaceBGs.OrderBy(x => rnd.Next());

        int closestSqr = 0;
        int remainder = 0;

        for (int i = 1; i < surfaceBGs.Length; i++)
        {
            int sqrTest = (int)Mathf.Pow(i, 2) - surfaceBGs.Length;
            if (sqrTest == 0)
            {
                closestSqr = i;
                break;
            }
            else if (sqrTest > 0)
            {
                closestSqr = i;
                remainder = sqrTest;
                break;
            }
        }

        int counter = 0;
        for (int i = 0; i < closestSqr; i++)
        {
            // init list
            List<Surface> newSurfaceList = new List<Surface>();
            surfaceGrid.Add(newSurfaceList);
            for (int j = 0; j < closestSqr; j++)
            {
                Surface s = InitSurface(counter, i, j, surfaceBGs[counter]);
                newSurfaceList.Add(s);
                counter++;
            }
        }
        // ad remainders
        //for (int i = 0; i < remainder; i++)
        //{
        //    surfaceGrid[i].Add((Texture2D)bgRends[counter].mainTexture);
        //    counter++;
        //}
    }


    Texture2D SplitPetPortion(int xOrigin, int yOrigin, int width, int height)
    {
        Texture2D output = new Texture2D(width, height);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color pixel = pet.GetPixel(i, j);
                output.SetPixel(i, j, pet.GetPixel(xOrigin + i, yOrigin + j));
            }
        }

        output.Apply();
        return output;
    }

    void CheckMoveBaseSurface()
    {
        // if moving completely to new canvas as base
        int xMove = offset.x - res.x;
        int yMove = offset.y - res.y;
        // x movement
        if (xMove > 0) // right
        {
            baseSurfaceLocation = GetConnectingSurfaceLocation(Vector2.right);
            offset = new Vector2Int(xMove, offset.y);
        }
        else if (xMove < -res.x) // left
        {
            baseSurfaceLocation = GetConnectingSurfaceLocation(Vector2.left);
            offset = new Vector2Int(res.x - (Mathf.Abs(xMove) - res.x), offset.y);
            //offset = new Vector2Int(Mathf.Abs(xMove), offset.y);
        }
        // y movement
        if (yMove > 0) // up
        {
            baseSurfaceLocation = GetConnectingSurfaceLocation(Vector2.up);
            offset = new Vector2Int(offset.x, yMove);
        }
        else if (yMove < -res.y) // down
        {
            baseSurfaceLocation = GetConnectingSurfaceLocation(Vector2.down);
            offset = new Vector2Int(offset.x, res.y - (Mathf.Abs(yMove) - res.y));
            //offset = new Vector2Int(offset.x, Mathf.Abs(yMove));
        }
    }

    Surface GetRenderSurface(Vector2Int location)
    {
        return surfaceGrid[location.x][location.y];
    }

    void ConfigurePetDivisions()
    {
        // ref to base surface
        Surface baseSurface = GetRenderSurface(baseSurfaceLocation);

        List<CanvasPlacement> drawPortions = new List<CanvasPlacement>();

        // overhang situation
        int xExtent = offset.x + pet.width - baseSurface.renderLayer.width;
        int yExtent = offset.y + pet.height - baseSurface.renderLayer.height;
        if (xExtent > 0 || yExtent > 0)
        {
            // 4 part split
            if (xExtent > 0 && yExtent > 0)
            {
                // bottom left pet portion
                Texture2D b_l_Pet = SplitPetPortion(0, 0, pet.width - xExtent, pet.height - yExtent);
                drawPortions.Add(new CanvasPlacement(baseSurface, offset, b_l_Pet));

                // bottom right pet portion
                Surface b_r_surface = GetRenderSurface(GetConnectingSurfaceLocation(Vector2.right));
                //Vector2Int b_r_Offset = new Vector2Int(0, b_r_surface.height - (pet.height - yExtent));
                Vector2Int b_r_Offset = new Vector2Int(0, offset.y);
                Texture2D b_r_Pet = SplitPetPortion(pet.width - xExtent, 0, xExtent, pet.height - yExtent);
                drawPortions.Add(new CanvasPlacement(b_r_surface, b_r_Offset, b_r_Pet));

                // top left pet portion
                Surface t_l_surface = GetRenderSurface(GetConnectingSurfaceLocation(Vector2.up));
                //Vector2Int t_l_Offset = new Vector2Int(t_l_surface.width - (pet.width - xExtent), 0);
                Vector2Int t_l_Offset = new Vector2Int(offset.x, 0);
                Texture2D t_l_Pet = SplitPetPortion(0, pet.height - yExtent, pet.width - xExtent, yExtent);
                drawPortions.Add(new CanvasPlacement(t_l_surface, t_l_Offset, t_l_Pet));

                // top right pet portion
                Surface t_r_surface = GetRenderSurface(GetConnectingSurfaceLocation(new Vector2(1, 1)));
                Vector2Int t_r_Offset = Vector2Int.zero;
                Texture2D t_r_Pet = SplitPetPortion(pet.width - xExtent, pet.height - yExtent, xExtent, yExtent);
                drawPortions.Add(new CanvasPlacement(t_r_surface, t_r_Offset, t_r_Pet));
            }
            // 2 part split
            else if (xExtent > 0)
            {
                // left pet portion
                Texture2D l_Pet = SplitPetPortion(0, 0, pet.width - xExtent, pet.height);
                drawPortions.Add(new CanvasPlacement(baseSurface, offset, l_Pet));

                // right pet portion
                Surface r_surface = GetRenderSurface(GetConnectingSurfaceLocation(Vector2.right));
                //Vector2Int r_Offset = new Vector2Int(pet.width - xExtent, 0);
                Vector2Int r_Offset = new Vector2Int(0, offset.y);
                Texture2D r_Pet = SplitPetPortion(pet.width - xExtent, 0, xExtent, pet.height);
                drawPortions.Add(new CanvasPlacement(r_surface, r_Offset, r_Pet));
            }
            else if (yExtent > 0)
            {
                // bottom pet portion
                Texture2D b_Pet = SplitPetPortion(0, 0, pet.width, pet.height - yExtent);
                drawPortions.Add(new CanvasPlacement(baseSurface, offset, b_Pet));

                // top pet portion
                Surface t_surface = GetRenderSurface(GetConnectingSurfaceLocation(Vector2.up));
                Vector2Int t_Offset = new Vector2Int(offset.x, 0);
                Texture2D t_Pet = SplitPetPortion(0, pet.height - yExtent, pet.width, yExtent);
                drawPortions.Add(new CanvasPlacement(t_surface, t_Offset, t_Pet));
            }
        }
        else
        {
            drawPortions.Add(new CanvasPlacement(baseSurface, offset, pet));
        }

        foreach (var item in drawPortions)
        {
            WritePetPixels(item.surface, item.petPortion, item.offset);
        }
    }

    void WritePixels(Texture2D surface, Texture2D toWrite, Vector2Int offset = new Vector2Int())
    {
        // write pet portion to material
        for (int i = 0; i < toWrite.width; i++)
        {
            for (int j = 0; j < toWrite.height; j++)
            {
                Color pixel = toWrite.GetPixel(i, j);
                if (pixel.a > 0.1f)
                {
                    surface.SetPixel(i + offset.x, j + offset.y, pixel);
                }
            }
        }
        // GPU 2 CPU
        surface.Apply();
    }

    void Pickup(Surface surfaceToWriteTo)
    {
        // surface has fruit?

        if (surfaceToWriteTo.fruitLoc.y != 0)
        {

            // player has reached fruit
            Rect petRect = new Rect(offset.x, offset.y, pet.width, pet.height);
            Rect fruitRect = new Rect(surfaceToWriteTo.fruitLoc.x, surfaceToWriteTo.fruitLoc.y, surfaceToWriteTo.fruitLoc.z, surfaceToWriteTo.fruitLoc.w);
            if (petRect.Overlaps(fruitRect))
            {
                Debug.Log("ID: " + surfaceToWriteTo.gridLoc + "pickup fruit loc: " + surfaceToWriteTo.fruitLoc);
                // clear fruit pixels
                surfaceToWriteTo.renderLayer.SetPixels32(renderColArray);
                // remove fruit location info
                surfaceToWriteTo.fruitLoc = new Vector4Class(0, 0, 0, 0);
                //SetFruitLocation(surfaceToWriteTo, zeroFruitPos);
                // remove base render layer color array
                SetBaseRenderLayerArray(surfaceToWriteTo, renderColArray);
                // scale up pet (all frames)
                foreach (var item in petFrames)
                {
                    TextureScaler.scale(item, item.width + growthRate, item.height + (int)(growthRate * petAspectRatio));
                }
            }
        }
    }

    void WritePetPixels(Surface surfaceToWriteTo, Texture2D petPortion, Vector2Int offset = new Vector2Int())
    {
        Pickup(surfaceToWriteTo);

        // reset canvas to blank wall
        if (!showTrail)
        {
            surfaceToWriteTo.renderLayer.SetPixels32(surfaceToWriteTo.baseRenderLayerArray);
        }

        // write pet portion to material
        for (int i = 0; i < petPortion.width; i++)
        {
            for (int j = 0; j < petPortion.height; j++)
            {
                Color pixel = petPortion.GetPixel(i, j);
                if (pixel.a > 0.1f)
                {
                    surfaceToWriteTo.renderLayer.SetPixel(i + offset.x, j + offset.y, pixel);
                }
            }
        }

        // GPU 2 CPU
        surfaceToWriteTo.renderLayer.Apply();
    }

    Vector2Int GetRandomOffset(Texture2D image)
    {
        int drawingX = image.width;
        int drawingY = image.height;

        int x = UnityEngine.Random.Range(0, res.x - drawingX);
        int y = UnityEngine.Random.Range(0, res.y - drawingY);

        return new Vector2Int(x, y);
    }

    void AnimateSprite()
    {
        spriteChangeCounter += Time.deltaTime;
        if (spriteChangeCounter > spriteChangeDelay)
        {
            if (petFrameIndex + 1 < petFrames.Count)
            {
                petFrameIndex++;
            }
            else
            {
                petFrameIndex = 0;
            }
            pet = petFrames[petFrameIndex];
            ConfigurePetDivisions();
            spriteChangeCounter = 0;
        }
    }

    void Update()
    {
        if (animated)
            AnimateSprite();

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            Vector2Int newOffset = new Vector2Int(offset.x, offset.y);
            int moveDelta = (int)(moveSpeed * Time.deltaTime);

            moveDelta = Mathf.Clamp(moveDelta, 1, 100);


            if (Input.GetKey(KeyCode.UpArrow))
            {
                newOffset = new Vector2Int(newOffset.x, newOffset.y + moveDelta);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                newOffset = new Vector2Int(newOffset.x, newOffset.y - moveDelta);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                newOffset = new Vector2Int(newOffset.x - moveDelta, newOffset.y);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                newOffset = new Vector2Int(newOffset.x + moveDelta, newOffset.y);
            }
            if (offset != newOffset)
            {
                //D apply new offset
                offset = newOffset;
                // check for change of base texture to render to
                CheckMoveBaseSurface();
                // figure out splitting and writing of texture
                ConfigurePetDivisions();
            }
        }
    }
}
