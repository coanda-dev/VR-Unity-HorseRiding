using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSV_Logger
{
    private string _filepath;
    public StreamWriter file;
    
    public CSV_Logger(string filepath)
    {
        _filepath = filepath;
        if (!File.Exists(filepath))
            file = File.CreateText(this._filepath);
        else
        {
            file = File.AppendText(this._filepath);
        }
    }

    ~CSV_Logger()
    {
        Close();
    }

    public void Close()
    {
        file.Close();
    }
}