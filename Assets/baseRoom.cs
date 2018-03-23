﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseRoom : MonoBehaviour {
    public static Texture2D handbeckon, handwag, dramamask, chatteringteeth, throbbingbrain, blueeye, browneye;
    protected static Texture2D currcursor;
    protected FMVManager fmvman;
    protected string myvidpath;
    //filenames for rotation seem to be f_4fc where the f=foyer, 4=the node number, f=forwards (b=backwards) or clockwise, the last letter is a/b/c/d and it means the orientation
    //for movement it's f1_2 where f=foyer, 1=starting node number, 2=destination node number
    //include the filename prefix in myvidpath, some are 1 letter (like f for foyer) and some are 2 letters (like bd for brian dutton)
    //not of all these seem to follow those rules, I think I'll have to use nodeNames to lookup filenames
    protected string[] nodeNames;//for convenience? I can just leave them as comments, but then I can't show them in debug output
    //string[] facingNames;
    public class RoomPosition
    {
        public int node;
        public char facing;//0=a, 1=b, 2=c, 3=d...?

        public RoomPosition(int n, char f)
        {
            node = n;
            facing = f;
        }
    };
    public RoomPosition currPos = new RoomPosition(1, 'a');
    //public int currNode=1;
    //public char facing;//0=a, 1=b, 2=c, 3=d...
    public enum ClickboxType { MOVE, TURN, EXITROOM, PUZZLE, DRAMAMASK, CHATTERINGTEETH };
    protected class NodeConnection
    {
        public RoomPosition fromPos;
        public Rect clickbox;
        public ClickboxType type;
        public RoomPosition[] toPos;
        public int timesClicked = 0;
        public System.Action<NodeConnection> callback = null;
    };
    protected List<NodeConnection> nodeConnections;

    protected void BaseInit()
    {
        if (nodeConnections != null)
        {
            Debug.Log("double init?");
            return;
        }

        fmvman = GameObject.FindObjectOfType<FMVManager>();
        //Debug.Log(fmvman.ToString());
        nodeConnections = new List<NodeConnection>();
        currPos.node = 1;

        handwag = Instantiate(Resources.Load("cursors/wagging-hand", typeof(Texture2D))) as Texture2D;
        handbeckon = Instantiate(Resources.Load("cursors/beckon", typeof(Texture2D))) as Texture2D;
        dramamask = Instantiate(Resources.Load("cursors/drama-mask", typeof(Texture2D))) as Texture2D;
        chatteringteeth = Instantiate(Resources.Load("cursors/chattering-teeth", typeof(Texture2D))) as Texture2D;
        throbbingbrain = Instantiate(Resources.Load("cursors/throbbing-skull", typeof(Texture2D))) as Texture2D;
        blueeye = Instantiate(Resources.Load("cursors/blue-eyeball", typeof(Texture2D))) as Texture2D;
        browneye = Instantiate(Resources.Load("cursors/brown-eyeball", typeof(Texture2D))) as Texture2D;
        SetCursor(handwag);
    }

	// Use this for initialization
	void Start () {
        BaseInit();
    }

    protected void SetCursor(Texture2D c)
    {
        if (currcursor == c) return;
        Cursor.SetCursor(c, Vector2.zero, CursorMode.Auto);
        currcursor = c;
    }

    protected void Update()
    {
        if(fmvman.playlist.Count>0)
        {
            SetCursor(handwag);
            return;
        }
        Vector2 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        var nc = GetNodeConnection(pos);
        if (nc == null)
        {
            SetCursor(handwag);
        }
        else if (nc.type == ClickboxType.DRAMAMASK)
        {
            SetCursor(dramamask);
        }
        else if (nc.type == ClickboxType.CHATTERINGTEETH)
        {
            SetCursor(chatteringteeth);
        }
        else if (nc.type == ClickboxType.PUZZLE)
        {
            SetCursor(throbbingbrain);
        }
        else
        {
            SetCursor(handbeckon);
        }
        if (Input.GetMouseButtonDown(0))
        {
            OnClick(pos);
        }
    }

    protected void CreateNodeConnection(RoomPosition fromPos, RoomPosition toPos, Rect clickbox, RoomPosition[] toPosArray=null)
    {
        var nc = new NodeConnection { fromPos=fromPos, clickbox = clickbox, type=ClickboxType.MOVE };
        if(toPosArray != null)
        {
            nc.toPos = toPosArray;
        } else
        {
            nc.toPos = new RoomPosition[1];
            nc.toPos[0] = toPos;
        }
        if (fromPos.node == nc.toPos[nc.toPos.Length-1].node) nc.type = ClickboxType.TURN;
        nodeConnections.Add( nc );
        MakeClickboxes();
    }

    protected void CreateNodeConnection(int from, int to, Rect clickbox, char fromFacing, char toFacing, char before, char after)
    {
        RoomPosition fromPos = new RoomPosition(from, fromFacing);
        RoomPosition[] toPosArray = new RoomPosition[3] { new RoomPosition(from, before), new RoomPosition(to, toFacing), new RoomPosition(to, after) };
        CreateNodeConnection(fromPos, toPosArray[0], clickbox, toPosArray);
    }

    protected void CreateNodeConnectionRotations(int from, char fromFacing, char toFacing)
    {
        Rect left = new Rect(0, 0, 0.02f, 1.0f);
        Rect right = new Rect(0.98f, 0, 0.02f, 1.0f);

        for (char f=fromFacing; f<toFacing;f++)
        {
            RoomPosition posLeft = new RoomPosition(from, f);
            RoomPosition posRight = new RoomPosition(from, (char)((int)f + 1));
            CreateNodeConnection(posRight, posLeft, left);
            CreateNodeConnection(posLeft, posRight, right);
        }
        RoomPosition fromPos = new RoomPosition(from, fromFacing);
        RoomPosition toPos = new RoomPosition(from, toFacing);
        CreateNodeConnection(fromPos, toPos, left);
        CreateNodeConnection(toPos, fromPos, right);
    }

    void MakeClickboxes()
    {
        return;
    }

    protected NodeConnection GetNodeConnection(Vector2 pos)
    {
        foreach (var nc in nodeConnections)
        {
            //Debug.Log(nc.ToString());
            if (nc.fromPos.node == currPos.node && nc.fromPos.facing==currPos.facing && nc.clickbox.Contains(pos))
            {
                return nc;
            }
        }
        return null;
    }

    public void Travel(int to, char toFacing)
    {
        Debug.Log("going from node " + currPos.node.ToString() + "-"+currPos.facing+" to " + to.ToString()+"-"+toFacing);
        if (currPos.node != to)
        {
            //Debug.Log("going from node " + currNode.ToString() + " to " + to.ToString());
            fmvman.QueueVideo(myvidpath + currPos.node.ToString() + "_" + to.ToString() + ".avi");
            currPos.node = to;
            currPos.facing = toFacing;
        }
        else //rotation
        {
            if (currPos.facing + 1 == toFacing || currPos.facing > toFacing + 1)//f
            {
                fmvman.QueueVideo(myvidpath + "_" + currPos.node.ToString() + "f" + currPos.facing + ".avi");//the underscore won't always be there?
            }
            else //b
            {
                fmvman.QueueVideo(myvidpath + "_" + currPos.node.ToString() + "b" + toFacing + ".avi");//the underscore won't always be there?
            }
            currPos.facing = toFacing;
        }
    }

    public void OnClick(Vector2 pos)
    {
        Debug.Log("clicky! "+pos.ToString()+" from "+ currPos.node.ToString()+" "+ currPos.facing);
        if (fmvman.playlist.Count > 0)
        {
            Debug.Log("speeeed boost! queue full");
            var p = fmvman.playlist.ToArray();
            fmvman.playlist.Clear();
            foreach(var c in p)
            {
                c.playbackSpeed = 4;
                fmvman.playlist.Enqueue(c);
            }
            return;
        }
        /*if(fmvman.playlist.Count > 3)
        {
            Debug.Log("queue full!");
            return;
        }*/

        var nc = GetNodeConnection(pos);
        if(nc!=null)
        {
            nc.timesClicked++;
            if(nc.toPos!=null) foreach(var f in nc.toPos)
                {
                    if(currPos!=f) Travel(f.node, f.facing);
                }
            if(nc.callback!=null)
            {
                nc.callback(nc);
            }
        }
    }

    protected void SetPosition(int node)
    {
        currPos.node = node;
        MakeClickboxes();
    }

    protected void MoveToPosition(int node)
    {
    }

    protected void QueueVideo(string file)
    {
        fmvman.QueueVideo(myvidpath+file);
    }
}
