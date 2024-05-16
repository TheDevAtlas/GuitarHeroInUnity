using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SongController : MonoBehaviour
{
    // Get A Folder With The Song Files //
    // Get The Album Cover - image file //
    // Get Song Data - .chart file //
    // Load In Notes - object //
    // Spawn Notes In After X Ticks //

    public string songFolder = "C:/Users/jacob/Desktop/Noisestorm - Crab Rave (Jak544)/Noisestorm - Crab Rave (Jak544)";

    public Image albumCoverGUI;

    public TextMeshProUGUI songName;
    public TextMeshProUGUI artist;
    public TextMeshProUGUI charter;
    public TextMeshProUGUI year;

    public List<Note> notes;

    // Sample values for BPM and resolution
    public float currentBPM = 120.0f;  // Beats per minute
    public int resolution = 192;       // Ticks per beat

    public float bpmStartTime = 0;     // Time when the current BPM started
    public float bpmStartTick = 0;     // Tick when the current BPM started

    public float songStartTime = 0f;

    public GameObject noteObj;
    public Material[] noteMaterials;
    public Transform[] notePos;

    AudioSource audioSource;

    public float delay = 5f;

    public void Start()
    {
        notes = new List<Note>();
        audioSource = GetComponent<AudioSource>();

        // Open Album Cover Image //
        LoadChartCover();

        // Chart File - Get Song Data //
        LoadChartData();

        // Chart File - Get Note Data //
        LoadNotes();

        // Start Song //
        StartCoroutine(PlaySong());
        PlaySongAudioAsync();
    }

    IEnumerator PlaySong()
    {
        yield return null;

        songStartTime = Time.time;  // Record the start time of the song

        foreach (var note in notes)
        {
            float noteTime = TicksToSeconds(note.tick);  // Convert tick to time
            float waitTime = noteTime - (Time.time - songStartTime);  // Calculate how long to wait

            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);  // Wait for the right time to spawn the note
            }

            SpawnNote(note);  // Spawn the note
        }
    }

    async void PlaySongAudioAsync()
    {
        string path = Path.Combine(songFolder, "song.wav");
        audioSource.clip = await LoadClip(path);

        StartCoroutine(ClipPlay());
    }

    IEnumerator ClipPlay()
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play();
    }

    [Obsolete]
    async Task<AudioClip> LoadClip(string path)
    {
        AudioClip clip = null;
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
        {
            _ = uwr.SendWebRequest();

            // wrap tasks in try/catch, otherwise it'll fail silently
            try
            {
                while (!uwr.isDone) await Task.Delay(5);

                if (uwr.isNetworkError || uwr.isHttpError) Debug.Log($"{uwr.error}");
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
            catch (Exception err)
            {
                Debug.Log($"{err.Message}, {err.StackTrace}");
            }
        }

        return clip;
    }

    void SpawnNote(Note note)
    {
        // Implement the logic to spawn a note in the game
        // Debug.Log($"Spawning note {note.noteType} at tick {note.tick}");

        GameObject newNote;
        int t = 0;

        switch (note.noteType)
        {
            case Note.NoteType.green:
                t = 0;
                break;
            case Note.NoteType.red:
                t = 1;
                break;
            case Note.NoteType.yellow:
                t = 2;
                break;
            case Note.NoteType.blue:
                t = 3;
                break;
            case Note.NoteType.orange:
                t = 4;
                break;
        }

        newNote = Instantiate(noteObj, notePos[t].position, notePos[t].rotation);
        newNote.GetComponent<NoteController>().mat = noteMaterials[t];
    }

    void LoadChartCover()
    {
        string coverPath = Path.Combine(songFolder, "album.jpg");
        if (File.Exists(coverPath))
        {
            byte[] fileData = File.ReadAllBytes(coverPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            Sprite albumCover = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            if (albumCoverGUI != null)
                albumCoverGUI.sprite = albumCover;
        }
    }

    void LoadChartData()
    {
        string chartPath = Path.Combine(songFolder, "notes.chart");
        if (File.Exists(chartPath))
        {
            string[] lines = File.ReadAllLines(chartPath);
            bool isInSongSection = false;
            foreach (var line in lines)
            {
                if (line.Trim() == "[Song]")
                {
                    isInSongSection = true;
                }
                else if (line.Trim().StartsWith("[") && isInSongSection)
                {
                    // We've reached the end of the song metadata section
                    break;
                }

                if (isInSongSection)
                {
                    if (line.StartsWith("  Name ="))
                        songName.text = line.Split('=')[1].Trim().Trim('"');
                    else if (line.StartsWith("  Artist ="))
                        artist.text = line.Split('=')[1].Trim().Trim('"');
                    else if (line.StartsWith("  Charter ="))
                        charter.text = line.Split('=')[1].Trim().Trim('"');
                    else if (line.StartsWith("  Year ="))
                        year.text = line.Split('=')[1].Trim().Trim('"').Trim(',').Trim(' ');
                }
            }

            // Song delay timer //

            lines = File.ReadAllLines(chartPath);
            isInSongSection = false;
            foreach (var line in lines)
            {
                if (line.Trim() == "[SyncTrack]")
                {
                    isInSongSection = true;
                }
                else if (line.Trim().StartsWith("[") && isInSongSection)
                {
                    // We've reached the end of the song metadata section
                    break;
                }

                if (isInSongSection)
                {
                    if (line.StartsWith("  0 = TS "))
                    {
                        delay = line.Split("  0 = TS ")[1].Trim(' ')[0] / 14f;
                    }
                        
                }
            }

            // Song bpm

            lines = File.ReadAllLines(chartPath);
            isInSongSection = false;
            foreach (var line in lines)
            {
                if (line.Trim() == "[SyncTrack]")
                {
                    isInSongSection = true;
                }
                else if (line.Trim().StartsWith("[") && isInSongSection)
                {
                    // We've reached the end of the song metadata section
                    break;
                }

                if (isInSongSection)
                {
                    if (line.StartsWith("  0 = B "))
                    {
                        //currentBPM = line.Split("  0 = B ")[1].Trim(' ')[0];
                    }

                }
            }
        }
    }

    void LoadNotes()
    {
        string chartPath = Path.Combine(songFolder, "notes.chart");
        if (File.Exists(chartPath))
        {
            string[] lines = File.ReadAllLines(chartPath);
            bool isNoteSection = false;
            notes = new List<Note>();

            foreach (var line in lines)
            {
                if (line.Trim() == "[ExpertSingle]")
                {
                    isNoteSection = true;
                    continue;
                }
                
                if (isNoteSection)
                {
                    if (line.Trim().StartsWith("["))
                        break; // End of note section

                    // Process the note line
                    if (line.Contains(" = N "))
                    {
                        string[] parts = line.Split(new[] { " = N " }, System.StringSplitOptions.RemoveEmptyEntries);
                        float tick = float.Parse(parts[0].Trim());
                        string[] noteParts = parts[1].Split(' ');
                        int noteKey = int.Parse(noteParts[0]);
                        float duration = float.Parse(noteParts[1]);

                        Note note = new Note
                        {
                            tick = tick,
                            noteType = Note.GetNoteType(noteKey),
                            noteDurration = duration
                        };
                        notes.Add(note);
                        
                    }
                }
            }
        }
    }

    // Converts ticks to seconds based on the current BPM and resolution
    public float TicksToSeconds(float ticks)
    {
        float secondsPerBeat = 60 / currentBPM;
        float deltaTicks = ticks - bpmStartTick;
        float deltaBeats = deltaTicks / resolution;
        float deltaSeconds = deltaBeats * secondsPerBeat;
        return deltaSeconds + bpmStartTime;
    }

    // Converts seconds to ticks based on the current BPM and resolution
    public float SecondsToTicks(float seconds)
    {
        float secondsPerBeat = 60 / currentBPM;
        float deltaSeconds = seconds - bpmStartTime;
        float deltaBeats = deltaSeconds / secondsPerBeat;
        float deltaTicks = deltaBeats * resolution;
        return deltaTicks + bpmStartTick;
    }
}

public struct Note
{
    public enum NoteType
    {
        green,  // Assume 0
        red,    // Assume 1
        yellow, // Assume 2
        blue,   // Assume 3
        orange  // Assume 4
    }

    public float tick;
    public NoteType noteType;
    public float noteDurration;

    public static NoteType GetNoteType(int key)
    {
        switch (key)
        {
            case 0: return NoteType.green;
            case 1: return NoteType.red;
            case 2: return NoteType.yellow;
            case 3: return NoteType.blue;
            case 4: return NoteType.orange;
            default: return NoteType.green; // Default case if note key is unrecognized
        }
    }
}