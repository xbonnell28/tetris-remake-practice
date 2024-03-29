﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    public Vector3 rotationPoint;
    private float previousTime;
    public static float fallTime = 0.8f;
    public static int height = 20;
    public static int width = 10;
    private static Transform[,] grid = new Transform[width, height];

    public AudioSource source;
    public AudioClip blockSound;
    public AudioClip deleteLine;

    // Start is called before the first frame update
    void Start()
    {

    }

    private IEnumerator playBlockSound()
    {
        Debug.Log("Entered BlockSound");
        source = GetComponent<AudioSource>();
        source.clip = blockSound;
        source.Play();
        yield return new WaitForSeconds(source.clip.length);
    }
    private IEnumerator playDeleteSound()
    {
        Debug.Log("Entered DeleteSound");
        source = GetComponent<AudioSource>();
        source.clip = deleteLine;
        source.volume = 0.25f;
        source.Play();
        yield return new WaitForSeconds(source.clip.length);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow)) // Used so the action only occurs once on a key press, holding down wont effect is
        {
            transform.position += new Vector3(-1, 0, 0);
            if (!ValidMove())
                transform.position -= new Vector3(-1, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += new Vector3(1, 0, 0);
            if (!ValidMove())
            {
                transform.position -= new Vector3(1, 0, 0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
            if (!ValidMove())
            {
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            FindObjectOfType<PieceGrabber>().StorePiece(this);
        }

        if(Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10 : fallTime)) // Used so as long as an input is received this action occurs, allows for held down actions
        {
            transform.position += new Vector3(0, -1, 0);
            if (!ValidMove())
            {
                transform.position -= new Vector3(0, -1, 0);
                AddToGrid();
                CheckForLines();

                this.enabled = false;
                if (!GameOver())
                {
                    FindObjectOfType<PieceGrabber>().GrabNextPiece();
                }
                else
                {
                    FindObjectOfType<SpawnTetromino>().ShowEndGameScreen();
                }
            }
            previousTime = Time.time;
        }
    }

    public void IncreaseSpeed()
    {
        fallTime = fallTime / 2;
    }

    bool GameOver()
    {
        for (int i = height - 1; i >= 16; i--){
            for (int j = 0; j < width; j++)
            {
                if (grid[j,i] != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void CheckForLines()
    {
        for (int i = height - 1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                DeleteLine(i);
                StartCoroutine(playDeleteSound());
                RowDown(i);
                FindObjectOfType<SpawnTetromino>().UpdateScore();
                FindObjectOfType<SpawnTetromino>().SetScoreText();
            }
        }
    }

    bool HasLine(int i)
    {
        for(int j = 0; j < width; j++)
        {
            if(grid[j,i] == null)
            {
                return false;
            }
        }

        return true;
    }

    void DeleteLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            Destroy(grid[j, i].gameObject);
            grid[j, i] = null;
        }
    }

    void RowDown(int i)
    {
        for (int y = i; y < height; y++)
        {
            for (int j = 0; j < width; j++)
            {
                if(grid[j,y] != null)
                {
                    grid[j, y - 1] = grid[j, y];
                    grid[j, y] = null;
                    grid[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }

    void AddToGrid()
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            grid[roundedX, roundedY] = children;
        }
        StartCoroutine(playBlockSound());
    }

    bool ValidMove()
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            if(roundedX < 0 || roundedX >= width || roundedY < 0 || roundedY >= height)
            {
                return false;
            }

            if(grid[roundedX, roundedY] != null)
            {
                return false;
            }
        }

        return true;
    }
}
