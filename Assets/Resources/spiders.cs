﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spiders : basePuzzle {

    //spider moving files are f_a_d.avi to f_h_e.avi, with a being the top position and b being the position to the left, going counter-clockwise
    //foy_spa is the video file for a spider appearing at point a
    //audio
    //2_s_2 (intro, play from foyer?), 2_s_3, 2_e_4, 2_e_3, gen_s_2("curses!"), gen_e_8 ("which way should I go now?")
    // Use this for initialization
    int currSpider = -1;
    bool[] spiderSpots;

    void Start () {
        BaseInit();
        myvidpath = "FH/f";
        //fmvman.QueueSong(whichway);
        PlaySong("../music/GU8.ogg");
        AddPuzzlePoint("a", new Rect(0.46f, 0.82f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("b", new Rect(0.33f, 0.72f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("c", new Rect(0.28f, 0.49f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("d", new Rect(0.32f, 0.26f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("e", new Rect(0.46f, 0.15f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("f", new Rect(0.6f, 0.25f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("g", new Rect(0.66f, 0.49f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("h", new Rect(0.6f, 0.72f, 0.05f, 0.05f), ClickPoint);
        spiderSpots = new bool[8];
        Debug.Log(spiderSpots[0]);
    }
	
	// Update is called once per frame
	/*void Update () {
        fmvman.QueueSong("GAMWAV/gen_s_2", true);
        endPuzzle("end");
        Destroy(this);
    }*/

    /*bool SpotHasSpider(char spot)
    {
        int i = (int)spot - (int)'a';
        return spiderSpots[i];
    }

    void SpotAddSpider(char spot)
    {
        int i = (int)spot - (int)'a';
        spiderSpots[i] = true;
    }*/

    int SpotNameToInt(string spot)
    {
        return (int)spot[0] - (int)'a';
    }

    string IntToSpotName(int spot)
    {
        char c = (char)((int)'a' + spot);
        Debug.Log(c.ToString());
        return c.ToString();
    }

    void ClickPoint(PuzzlePoint pp)
    {
        if (pp == null) return;

        int spot = SpotNameToInt(pp.name);
        if(spiderSpots[spot])
        {
            Debug.Log("spot already filled");
            return;
        }
        if (currSpider == -1)
        {
            if ( spiderSpots[(spot + 3) % 8] && spiderSpots[(spot + 5) % 8] )
            {
                Debug.Log("spot invalid");
                return;
            }
            if(fmvman.playing_audio.Count==0) PlaySound(whichway);
            QueueOverlay(myvidpath + "oy_sp" + pp.name + ".avi", null, new Color(0, 0, 0, 1), "spider-" + pp.name, true);
            currSpider = spot;
        } else
        {
            //skipping 3s and skipping 5s, perhaps that's how one derives the spiders for this starry tale
            if(spot==(currSpider+3)%8 || spot==(currSpider+5)%8 )
            {
                fmvman.ClearPlayingVideos("spider-" + IntToSpotName(currSpider));
                spiderSpots[spot] = true;
                int spidersCount = 0;
                foreach (var s in spiderSpots)
                {
                    if (s) spidersCount++;
                }
                System.Action<FMVManager.Command> callback = SpiderArrived;
                if (spidersCount == 7)
                {
                    callback = SpiderArrivedWin;
                }
                if(spidersCount == 2) PlaySound("GAMWAV/2_s_3.avi");
                if(spidersCount == 4) PlaySound("GAMWAV/2_e_4.avi");
                if (spidersCount == 6) PlaySound("GAMWAV/2_e_3.avi");
                QueueOverlay(myvidpath + "_" + IntToSpotName(currSpider) + "_" + pp.name + ".avi", callback, new Color(0, 0, 0, 1));
                currSpider = -1;
            }
        }
    }

    void SpiderArrived(FMVManager.Command c)
    {

    }

    void SpiderArrivedWin(FMVManager.Command c)
    {
        Debug.Log("You Win!");
        PlaySound("GAMWAV/gen_s_2.avi", EndCurses, true);//curses!
    }

    void EndCurses(FMVManager.Command c)
    {
        if (endPuzzle != null) endPuzzle("spiders");
        Destroy(this);
    }
}
