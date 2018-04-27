﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cans : basePuzzle
{
	// Use this for initialization
	void Start () {
        BaseInit();
        myvidpath = "K/";
        PlaySong("GU17");//which song?
        fmvman.QueueVideo(new FMVManager.Command { file = "K/shelf.avi", tags = "shelf puzzle", fadeInTime = 1.0f, callback = EndCans });
    }

    void EndCans(FMVManager.Command c)
    {
        fmvman.ClearPlayingVideos("puzzle");
        if (endPuzzle != null) endPuzzle("cans");
        Destroy(this.gameObject);
    }	
}
