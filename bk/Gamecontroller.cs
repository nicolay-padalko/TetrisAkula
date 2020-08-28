using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamecontroller : MonoBehaviour {

    Board m_gameBoard;

    Spawner m_spawner;

    Shape m_activeShape;

    SoundManager m_soundManager;

    ScoreManager m_scoreManager;

    Ghost m_ghost;

    public float m_dropInterval = 0.5f;

    float m_timeToDrop;

    float m_timeToNextKey;

    [Range(0.02f,1)]
    public float m_keyRepeatRate = 0.25f;

    float m_timeToNextKeyLeftRight;

    [Range(0.02f,1f)]
    public float m_keyRepeatRateLeftRight = 0.15f;

    float m_timeToNextKeyDown;

    [Range(0.01f,1f)]
    public float m_keyRepeatRateDown = 0.01f;

    float m_timeToNextKeyRotate;

    [Range(0.02f, 1f)]
    public float m_keyRepeatRateRotate = 0.25f;

    bool m_gameOver = false;

    public GameObject m_gameOverPanel;

    public IconToggle m_rotIconToggle;

    bool m_clockwise = true;

    public bool m_isPaused = false;

    public GameObject m_pausePanel;



    // Use this for initialization 
    void Start () {

        m_gameBoard = GameObject.FindObjectOfType<Board>();
        m_spawner = GameObject.FindObjectOfType<Spawner>();
        m_soundManager = GameObject.FindObjectOfType<SoundManager>();
        m_scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        m_ghost = GameObject.FindObjectOfType<Ghost>();


        m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;
        m_timeToNextKeyDown = Time.time + m_keyRepeatRateDown;
        m_timeToNextKeyRotate = Time.time + m_keyRepeatRateRotate;

        if (!m_gameBoard)
        {
            Debug.LogWarning("ATENÇÃO não ha objeto Game board definido");
        }

        if (!m_soundManager)
        {
            Debug.LogWarning("ATENÇÃO! não objeto definido como sound manager");
        }

        if (!m_scoreManager)
        {
            Debug.LogWarning("ATENÇÃO! não objeto definido como score manager");
        }

        if (!m_spawner)
        {
            Debug.LogWarning("ATENÇÃO não ha objeto Spawner definido");
        }
        else
        {
            m_spawner.transform.position = Vectorf.Round(m_spawner.transform.position);
            if (!m_activeShape)
            {
                m_activeShape = m_spawner.SpawnShape();
            }
        }

        if (m_gameOverPanel)
        {
            m_gameOverPanel.SetActive(false);
        }

        if (m_pausePanel)
        {
            m_pausePanel.SetActive(false);
        }

        if (m_soundManager.m_fxEnabled && m_soundManager.m_moveSound)
        {
            AudioSource.PlayClipAtPoint(m_soundManager.m_moveSound, Camera.main.transform.position, m_soundManager.m_fxVolume);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!m_gameBoard || !m_spawner || !m_activeShape || m_gameOver || !m_soundManager || !m_scoreManager)
        {
            return;
        }
        PlayerInput();

    }

    void LateUpdate()
    {
        if (m_ghost)
        {
            m_ghost.DrawGhost(m_activeShape,m_gameBoard);
        }
    }

    private void PlayerInput()
    {
        if (Input.GetButton("MoveRight") && (Time.time > m_timeToNextKeyLeftRight) || Input.GetButtonDown("MoveRight"))
        {
            m_activeShape.MoveRight();
            m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;
            
            if (!m_gameBoard.IsValidPosition(m_activeShape))
            {
                m_activeShape.MoveLeft();
                PlaySound(m_soundManager.m_errorSound,0.5f);
            }
            else
            {
                PlaySound (m_soundManager.m_moveSound,0.5f);
            }
        }

        else if (Input.GetButton("MoveLeft") && (Time.time > m_timeToNextKeyLeftRight) || Input.GetButtonDown("MoveLeft"))
        {
            m_activeShape.MoveLeft();
            m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;

            if (!m_gameBoard.IsValidPosition(m_activeShape))
            {
                m_activeShape.MoveRight();
                PlaySound(m_soundManager.m_errorSound, 0.5f);
            }
            else
            {
                PlaySound(m_soundManager.m_moveSound, 0.5f);
            }
        }

        else if (Input.GetButtonDown("Rotate") && (Time.time > m_timeToNextKeyRotate))
        {
            //m_activeShape.RotateRight();
            m_activeShape.RotateClockwise(m_clockwise);
            m_timeToNextKeyRotate = Time.time + m_keyRepeatRateRotate;

            if (!m_gameBoard.IsValidPosition(m_activeShape))
            {
                // m_activeShape.RotateLeft();
                m_activeShape.RotateClockwise(!m_clockwise);
                PlaySound(m_soundManager.m_errorSound, 0.5f);
            }
            else
            {
                PlaySound(m_soundManager.m_moveSound, 0.5f);
            }
        }

        else if (Input.GetButton("MoveDown") && (Time.time > m_timeToNextKeyDown) || (Time.time > m_timeToDrop))
        {
            m_timeToDrop = Time.time + m_dropInterval;
            m_timeToNextKeyDown = Time.time + m_keyRepeatRateDown;

            m_activeShape.MoveDown();
            if (!m_gameBoard.IsValidPosition(m_activeShape))
            {
                if (m_gameBoard.IsOverLimit(m_activeShape))
                {
                    GameOver();
                }
                else
                {
                    LandShape();
                }
                
            }
        }
        else if (Input.GetButtonDown("ToggleRot"))
        {
            ToggleRotDirection();
        }
    }

    void PlaySound (AudioClip clip, float volMultiplier = 1.0f)
    {
        if (clip && m_soundManager.m_fxEnabled)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, Mathf.Clamp( m_soundManager.m_fxVolume * volMultiplier, 0.05f,1f));
        }
    }

    void GameOver()
    {
        m_activeShape.MoveUp();
        
       
        if (m_gameOverPanel)
        {
            m_gameOverPanel.SetActive(true);
        }

        PlaySound(m_soundManager.m_gameOverSound, 2f);
        PlaySound(m_soundManager.m_gameOverVocalClip, 5f);

        m_gameOver = true;
    }

    void LandShape()
    {
        m_timeToNextKeyLeftRight = Time.time;
        m_timeToNextKeyDown = Time.time;
        m_timeToNextKeyRotate = Time.time;

        m_activeShape.MoveUp();
        m_gameBoard.StoreShapeInGrid(m_activeShape);

        if (m_ghost)
        {
            m_ghost.Reset();
        }

        m_activeShape = m_spawner.SpawnShape();

        m_gameBoard.ClearAllRows();

        PlaySound(m_soundManager.m_dropSound,0.75f);

        if (m_gameBoard.m_completedRows >0)
        {
            m_scoreManager.ScoreLines(m_gameBoard.m_completedRows);

            if (m_gameBoard.m_completedRows > 1)
            {
                AudioClip randomVocal = m_soundManager.GetRandomClip(m_soundManager.m_vocalClips);
                PlaySound(randomVocal);
            }
            PlaySound(m_soundManager.m_clearRowSound);
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Application.LoadLevel(Application.loadedLevel);
    }

    public void ToggleRotDirection()
    {
        m_clockwise = !m_clockwise;
        if (m_rotIconToggle)
        {
            m_rotIconToggle.ToggleIcon(m_clockwise);
        }

    }

    public void TogglePause()
    {
        m_isPaused = !m_isPaused;


        if (m_pausePanel)
        {
            m_pausePanel.SetActive(m_isPaused);
            if (m_soundManager)
            {
                m_soundManager.m_musicSource.volume = (m_isPaused) ? m_soundManager.m_musicVolume * 0.25f : m_soundManager.m_musicVolume;
            }

            Time.timeScale = (m_isPaused) ? 0 : 1;
           
        }
    }
}
