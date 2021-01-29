using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public static class MatrixEvaluation
    {
        public static int[] LinePays;
        public static List<int[]> LinePaysLogic;
        //Have this class take the Matrix and Set Line Pays Cycling information and Bet Amount Won.
        //Need to have Credits Meter (Informaiton held in PlayerInformation in Slot Engine) += Credits Won.Have Bet Won Meter Rack Down as Credits Meter Racks Up
        public static void SetMatrixEvaluation(Reel[] Matrix)
        {
            //Line Line Pays Array to check and check if 3-5 slots on the reels are usable
            //Need to add support for analysing reels left to right and right to left or

        }

        private static void GenerateAndSendLinePays()
        {
            //Connect to LinePaysManager
        }

        private static void ASynchronousLoadLinePaysData()
        {
            ParseLinePayLogicFile();
        }

        private static void ParseLinePayLogicFile()
        {
            //99paylines_3x5
            StreamReader rReader = new StreamReader(Application.dataPath + "/Resources/Data/" + SlotEngine._instance.iPayLines + "paylines_"+SlotEngine._instance.mMatrixType.ToString());
            string line;
            LinePaysLogic = new List<int[]>();
            char[] cSplitCondition = new char[1]{'|'};
            while ((line = rReader.ReadLine()) != null)
            {
                string[] sLogicToConvert = line.Split(cSplitCondition);
                int[] iConvertedLogic = new int[sLogicToConvert.Length];
                for (int i = 0; i < iConvertedLogic.Length; i++)
                {
                    iConvertedLogic[i] = (Convert.ToInt16(sLogicToConvert[i]) + SlotEngine._instance.iSlotCushionSpinning -1);
                    //You need to increase all evaluations to account for the cushion area (-1 for the ending cushion slot)
                }
                LinePaysLogic.Add(iConvertedLogic);
            }
            rReader.Close();
            // You should call Dispose on "reader" here, too.
            rReader.Dispose();
            Debug.Log("Line Pays Logic has been set for " + "Resources/Data/" + SlotEngine._instance.iPayLines + "paylines_"+SlotEngine._instance.mMatrixType.ToString());
        }

        /*void SwitchState()
        {

        }*/

    }

