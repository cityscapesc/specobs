﻿//============================================================================
//RF Explorer for Windows - A Handheld Spectrum Analyzer for everyone!
//Copyright © 2010-15 Ariel Rocholl, www.rf-explorer.com
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License.  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER
// EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE,
// FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RFExplorerCommunicator
{
    /// <summary>
    /// Class support a single collection of calibration amplitude values, of 1 MHz steps
    /// Positive values will be used to externally add to the measurement, that means imply correcting attenuation
    /// whereas negative values will be to externally substract to the measurement, implying correcting gain.
    /// </summary>
    public class RFEAmplitudeTableData
    {
        public const UInt16 MAX_ENTRY_DATA = 6101;
        public const UInt16 MIN_ENTRY_DATA = 0;
        public const float INVALID_DATA = -1E10f;
        private const float DEFAULT_COMPRESSION = -10f;
        private const float DEFAULT_AMPLITUDE_CORRECTION = 0f;

        bool m_bHasCompressionData = false;
        /// <summary>
        /// Returns true if data stored include compression amplitude for overload check
        /// </summary>
        public bool HasCompressionData
        {
            get { return m_bHasCompressionData; }
        }

        bool m_bHasCalibrationData = false;
        /// <summary>
        /// Will return true if there is loaded valid calibration data
        /// </summary>
        public bool HasCalibrationData
        {
            get { return m_bHasCalibrationData; }
        }

        string m_sCalibrationID = "";
        /// <summary>
        /// Calibration ID is usually a filename to name the calibration in use
        /// future versions may support different IDs than a filename
        /// </summary>
        public string CalibrationID
        {
            get { return m_sCalibrationID; }
        }

        /// <summary>
        /// Amplitude correction data for each MHZ entry
        /// </summary>
        public float[] m_arrAmplitudeCalibrationDataDB;
        public float GetAmplitudeCalibration(int nIndexMHz)
        {
            if ((nIndexMHz<MAX_ENTRY_DATA) && (m_arrAmplitudeCalibrationDataDB!=null)
                && (m_arrAmplitudeCalibrationDataDB.Length > nIndexMHz) && (m_arrAmplitudeCalibrationDataDB[nIndexMHz] != INVALID_DATA))
                return m_arrAmplitudeCalibrationDataDB[nIndexMHz];
            else
                return DEFAULT_AMPLITUDE_CORRECTION;
        }

        /// <summary>
        /// Amplitude compression data for each MHZ entry
        /// </summary>
        public float[] m_arrCompressionDataDBM;
        public float GetCompressionAmplitude(int nIndexMHz)
        {
            if ((nIndexMHz < MAX_ENTRY_DATA) && (m_arrCompressionDataDBM != null)
                && (m_arrCompressionDataDBM.Length > nIndexMHz) && (m_arrAmplitudeCalibrationDataDB[nIndexMHz] != INVALID_DATA))
                return m_arrCompressionDataDBM[nIndexMHz];
            else
                return DEFAULT_COMPRESSION;
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public RFEAmplitudeTableData()
        {
            m_arrAmplitudeCalibrationDataDB = new float[MAX_ENTRY_DATA];
            m_arrCompressionDataDBM= new float[MAX_ENTRY_DATA];
            m_bHasCompressionData = false;
            Clear();
        }

        private string FileHeaderVersioned()
        {
            return "--RFEAT01";
        }

        private void Clear()
        {
            m_sCalibrationID = "";
            m_bHasCompressionData = false;
            m_bHasCalibrationData = false;
            for (int nInd=0; nInd<m_arrAmplitudeCalibrationDataDB.Length; nInd++)
            {
                m_arrAmplitudeCalibrationDataDB[nInd] = INVALID_DATA;
                m_arrCompressionDataDBM[nInd] = INVALID_DATA;
            }
        }

        /// <summary>
        /// utility function to be used by both arrays, when needed
        /// </summary>
        /// <param name="arrAmplitudeData"></param>
        private void NormalizeDataIterating(ref float[] arrAmplitudeData)
        {
            float fAmplitude1 = INVALID_DATA, fAmplitude2 = INVALID_DATA; //the two amplitude values to iterate in order to adjust intermediate values
            int nAmplitude1Ind = -1, nAmplitude2Ind = -1; //Index used to know the position of the two amplitudes

            for (int nInd = 0; nInd < arrAmplitudeData.Length; nInd++)
            {
                float fVal = arrAmplitudeData[nInd];

                if (fAmplitude1 == INVALID_DATA)
                {
                    if (fVal != INVALID_DATA)
                    {
                        fAmplitude1 = fVal;
                        nAmplitude1Ind = nInd;
                    }
                    else
                    {
                        //use DEFAULT_AMPLITUDE_CORRECTION if nothing else is found valid yet
                        arrAmplitudeData[nInd] = DEFAULT_AMPLITUDE_CORRECTION;
                    }
                }
                else if (fAmplitude2 == INVALID_DATA)
                {
                    if (fVal != INVALID_DATA)
                    {
                        fAmplitude2 = fVal;
                        nAmplitude2Ind = nInd;

                        if ((nAmplitude2Ind - nAmplitude1Ind) > 1)
                        {
                            //if more than one step is between the two, iterate to add an incremental delta
                            float fDelta = (fAmplitude2 - fAmplitude1) / (nAmplitude2Ind - nAmplitude1Ind);
                            int nSteps = 1;
                            for (int nInd2 = nAmplitude1Ind + 1; nInd2 < nAmplitude2Ind; nInd2++, nSteps++)
                            {
                                arrAmplitudeData[nInd2] = fAmplitude1 + nSteps * fDelta;
                            }
                        }

                        fAmplitude1 = fAmplitude2;
                        nAmplitude1Ind = nAmplitude2Ind;
                        fAmplitude2 = INVALID_DATA;
                        nAmplitude2Ind = 0;
                    }
                    else
                    {
                        //Use last valid value from now on, it should be overwritten and updated later, but if that was
                        //the last sample, then this will be good for the remaining of the samples
                        arrAmplitudeData[nInd] = fAmplitude1;
                    }
                }
            }
        }

        /// <summary>
        /// It will iterate to all values and will fill in anything that is not initialized with a valid value
        /// As oposed to NormalizeDataCopy, it will look for valid values and will fill it in with intermediate
        /// calculated values in between these two. If no valid value is found among two (i.e. last value or first value)
        /// then it is filled in using NormalizedDataCopy.
        /// </summary>
        private void NormalizeAmplitudeCalibrationDataIterating()
        {
            NormalizeDataIterating(ref m_arrAmplitudeCalibrationDataDB);
            //NormalizeDataIterating(ref m_arrCompressionData);
        }

        /// <summary>
        /// This function will make sure the compression data has start/end points even if not specified in the file
        /// </summary>
        private void NormalizeCompressionData()
        {
            if (m_arrCompressionDataDBM[MIN_ENTRY_DATA] == INVALID_DATA)
            {
                m_arrCompressionDataDBM[MIN_ENTRY_DATA] = DEFAULT_COMPRESSION;
            }
            if (m_arrCompressionDataDBM[MAX_ENTRY_DATA-1] == INVALID_DATA)
            {
                m_arrCompressionDataDBM[MAX_ENTRY_DATA - 1] = DEFAULT_COMPRESSION;
            }
        }

        /// <summary>
        /// It will iterate to all values and will fill in anything that is not initialized with a valid value
        /// It uses a copy method, not an incremental method (i.e. it will pick the first valid value and 
        /// go copying the same value over and over till it find another valid one. See NormalizeDataPredict for alternative
        /// </summary>
        private void NormalizeDataCopy()
        {
            float fLastAmplitude = DEFAULT_AMPLITUDE_CORRECTION;
            //float fLastCompression=0.0f;
            for (int nInd = 0; nInd < m_arrAmplitudeCalibrationDataDB.Length; nInd++)
            {
                float fVal=m_arrAmplitudeCalibrationDataDB[nInd];
                if (fVal == INVALID_DATA)
                {
                    m_arrAmplitudeCalibrationDataDB[nInd] = fLastAmplitude;
                }
                else
                {
                    fLastAmplitude = fVal;
                }

                //fVal = m_arrCompressionData[nInd];
                //if (fVal == INVALID_DATA)
                //{
                //    m_arrCompressionData[nInd] = fLastCompression;
                //}
                //else
                //{
                //    fLastCompression = fVal;
                //}
            }
        }

        /// <summary>
        /// Load a file with amplitude and optionally compression data
        /// </summary>
        /// <param name="sFilename">full path of the filename</param>
        /// <returns>true if everything ok, false if data was invalid</returns>
        public bool LoadFile(string sFilename)
        {
            bool bOk = true;

            using (StreamReader myFile = new StreamReader(sFilename))
            {
                string sHeader = myFile.ReadLine().TrimStart(' ');
                if (sHeader != FileHeaderVersioned())
                {
                    //unknown format
                    return false;
                }

                Clear();
                string sLine;
                do
                {
                    //Read line, trim and replace all consecutive blanks with a single tab
                    sLine = myFile.ReadLine().TrimStart(' ');
                    sLine = sLine.TrimEnd(' ');
                    sLine = sLine.Replace('\t', ' ');
                    while (sLine.Contains("  "))
                    {
                        sLine = sLine.Replace("  ", " ");
                    }

                    if (sLine.Substring(0, 2) != "--")
                    {
                        string[] arrStrings = sLine.Split(' ');
                        if (arrStrings.Length >= 2)
                        {
                            int nMHZ = Convert.ToInt16(arrStrings[0]);
                            m_arrAmplitudeCalibrationDataDB[nMHZ] = (float)Convert.ToDouble(arrStrings[1]);
                            if (arrStrings.Length >= 3)
                            {
                                //this is a file that includes compression data
                                m_arrCompressionDataDBM[nMHZ] = (float)Convert.ToDouble(arrStrings[2]);
                                m_bHasCompressionData = true;
                            }
                        }
                        else
                            bOk = false;
                    }
                }
                while (bOk && !myFile.EndOfStream);

                if (bOk)
                {
                    //update calibration file name
                    string[] sFile = sFilename.Split('\\');
                    if (sFile.Length > 0)
                    {
                        m_sCalibrationID = sFile[sFile.Length - 1].ToUpper().Replace(".RFA","");
                    }

                    //fill in all gaps
                    m_bHasCalibrationData = true;
                    NormalizeAmplitudeCalibrationDataIterating();
                    NormalizeCompressionData();
                }
                else
                {
                    Clear();
                }
            }

            return bOk;
        }
    }
}
