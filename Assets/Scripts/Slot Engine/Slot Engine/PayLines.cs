using UnityEngine;
using System.Collections;

//For Parsing Purposes
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//************

/// <summary>
/// This holds all payline information. Paylines are processed in the Slot Engine Script by cycling through the iPayLines and comparing whether symbols match on those paylines.
/// </summary>
public class PayLines
{

    Payline[] iPayLines;
	// Use this for initialization
	void SetPaylines(MatrixTypes Matrix) 
    {
        iPayLines = GeneratePaylines(Matrix);
	}

    Payline[] GeneratePaylines(MatrixTypes Matrix)
    {
        //Need to Change to generate Matrix type from matrix input
        StreamReader myFile = new StreamReader(Application.dataPath + "/Resources/Data/99paylines_3x5.txt");
        //Set the Length of the Paylines file to use
        iPayLines = new Payline[ReturnLengthStreamReader(myFile)];
        //Cache variable for the line being read
        string sPayLineUnParsed;

        int iPaylineNumber = 0;
        //While there is a line to read parse payline information
        while (myFile.Peek() >= 0)
        {
            iPaylineNumber++;
            sPayLineUnParsed = myFile.ReadLine();
            char[] ParseInformation = new char[1];
            ParseInformation[0] = '|';
            string[] payInfo = sPayLineUnParsed.Split(ParseInformation);
            int[] iPayInfo = new int[payInfo.Length];
            for (int i = 0; i < payInfo.Length; i++)
            {
                iPayInfo[i] = int.Parse(payInfo[i]);
            }
            iPayLines[iPaylineNumber].PopulatePayline(iPayInfo);
        }
        return null;
    }

    int ReturnLengthStreamReader(StreamReader Reader)
    {
        int i = 0;
        while (Reader.ReadLine() != null) { i++; }
        return i;
    }

}

public class Payline
{
    int[] iPaylineDefine;

    public void PopulatePayline(int[] Information)
    {
        iPaylineDefine = Information;
    }
}
