﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace AqVision.Acquistion
{
    public delegate void DelegateOnError(int id);
    public delegate void DelegateOnBitmap(string strBmpBase64);
    class AqAcquisitionImage
    {
        private event DelegateOnError m_EventOnError;
        private event DelegateOnBitmap m_EventOnBitmap;
        private bool m_GetBitmapSuc = false;
        private GCHandle m_gcError;
        private GCHandle m_gcBitmap;
        
        System.Drawing.Bitmap m_RevBitmap = null;

        public System.Drawing.Bitmap RevBitmap
        {
            get { return m_RevBitmap; }
            set { m_RevBitmap = value; }
        }


        public AqAcquisitionImage()
        {
            m_EventOnError += new DelegateOnError(OnError);
            m_gcError = GCHandle.Alloc(m_EventOnError);
            IntPtr ptr = Marshal.GetFunctionPointerForDelegate(m_EventOnError);
            AqVision.Interaction.UI2LibInterface.RegisterErrorFunction(ptr);

            m_EventOnBitmap += new DelegateOnBitmap(OnReceiveBitmap);
            m_gcBitmap = GCHandle.Alloc(m_EventOnBitmap);
            IntPtr ptrBitmap = Marshal.GetFunctionPointerForDelegate(m_EventOnBitmap);
            AqVision.Interaction.UI2LibInterface.RegisterBitmapFunction(ptrBitmap);
        }

        ~AqAcquisitionImage()
        {
            m_gcError.Free();
            m_gcBitmap.Free();
        }

        void OnReceiveBitmap(string strBmpBase64)
        {
            int ibmpLen = strBmpBase64.Length;
            m_RevBitmap = AqVision.Utility.Base64.Base64ToBitmap(strBmpBase64);
            AqVision.Interaction.UI2LibInterface.OutputDebugString("IntegrationTesting OnReceiveBitmap end ");
            m_GetBitmapSuc = true;
        }

        void OnError(int id)
        {
            //System.Windows.Forms.MessageBox.Show(id.ToString());
        }

        public bool Connect()
        {
            try
            {
                AqVision.Interaction.UI2LibInterface.Disconnect();
                AqVision.Interaction.UI2LibInterface.Connect();
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("IntegrationTesting Connect error " + ex.Message);
                 AqVision.Interaction.UI2LibInterface.OutputDebugString("IntegrationTesting Connect error " + ex.Message);
            }
            
            return true;
        }

        public bool DisConnect()
        {
            try
            {
                AqVision.Interaction.UI2LibInterface.Disconnect();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("IntegrationTesting DisConnect error " + ex.Message);
                AqVision.Interaction.UI2LibInterface.OutputDebugString("IntegrationTesting DisConnect error " + ex.Message);
            }
            
            return true;
        }

        public System.Drawing.Bitmap Acquisition()
        {
            try
            {
                m_GetBitmapSuc = false;
                AqVision.Interaction.UI2LibInterface.GetBitmap(1, 0);
                while (!m_GetBitmapSuc)
                {
                    Thread.Sleep(10);
                }
                return RevBitmap;
            }
            catch (Exception ex)
            {
                AqVision.Interaction.UI2LibInterface.OutputDebugString("IntegrationTesting get bitmp error " + ex.Message);
                return null;
            }
        }
    }
}
