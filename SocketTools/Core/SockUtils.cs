using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;

namespace SocketTool.Core
{
    public class SocketUtil
    {
        public static string LastError = string.Empty;

        private static Hashtable ErrorMsgMap = new Hashtable();
        public SocketUtil()
        {

           
        }

        /// <summary>
        /// Turn on keep alive on a socket.</summary>
        /// <param name="turnOnAfter">
        /// Specifies the timeout, in milliseconds, with no activity until the first keep-alive packet is sent
        /// <param name="keepAliveInterval">
        /// Specifies the interval in milliseconds to send the keep alive packet.</param>
        /// <remarks>The keepAliveInternal doesn't seem to do any difference!</remarks>
        public static bool SetKeepAlive(Socket socket, ulong turnOnAfter, ulong keepAliveInterval)
        {
            int bytesperlong = 4;   // in c++ a long is four bytes long
            int bitsperbyte = 8;

            try
            {
                // Enables or disables the per-connection setting of the TCP keep-alive option which 
                // specifies the TCP keep-alive timeout and interval. The argument structure for 
                // SIO_KEEPALIVE_VALS is specified in the tcp_keepalive structure defined in the Mstcpip.h 
                // header file. This structure is defined as follows: 
                // /* Argument structure for SIO_KEEPALIVE_VALS */
                // struct tcp_keepalive {
                //    u_long  onoff;
                //    u_long  keepalivetime;
                //    u_long  keepaliveinterval;
                //};
                // SIO_KEEPALIVE_VALS is supported on Windows 2000 and later.
                byte[] SIO_KEEPALIVE_VALS = new byte[3 * bytesperlong];
                ulong[] input = new ulong[3];

                // put input arguments in input array
                if (turnOnAfter == 0 || keepAliveInterval == 0) // enable disable keep-alive
                    input[0] = (0UL); // off
                else
                    input[0] = (1UL); // on

                input[1] = (turnOnAfter);
                input[2] = (keepAliveInterval); 

                // pack input into byte struct
                for (int i = 0; i < input.Length; i++)
                {
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 3] = (byte)(input[i] >> ((bytesperlong - 1) * bitsperbyte) & 0xff);
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 2] = (byte)(input[i] >> ((bytesperlong - 2) * bitsperbyte) & 0xff);
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 1] = (byte)(input[i] >> ((bytesperlong - 3) * bitsperbyte) & 0xff);
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 0] = (byte)(input[i] >> ((bytesperlong - 4) * bitsperbyte) & 0xff);
                }
                // create bytestruct for result (bytes pending on server socket)
                byte[] result = BitConverter.GetBytes(0);
                
                // write SIO_VALS to Socket IOControl
                socket.IOControl(IOControlCode.KeepAliveValues, SIO_KEEPALIVE_VALS, result);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        private static void SetErrorMsg()
        {
            ErrorMsgMap[10004] = "������ȡ��";
            ErrorMsgMap[10013] = "����ĵ�ַ��һ���㲥��ַ���������ñ�־";
            ErrorMsgMap[10014] = "��Ч�Ĳ���";
            ErrorMsgMap[10022] = "�׽���û�а󶨣���Ч�ĵ�ַ������������֮ǰ����";
            ErrorMsgMap[10024] = "û�и�����ļ������������ܶ����ǿյ�";
            ErrorMsgMap[10035] = "�׽����Ƿ������ģ�ָ���Ĳ�������ֹ";
            ErrorMsgMap[10036] = "һ��������Winsock�������ڽ�����";
            ErrorMsgMap[10037] = "������ɣ�û�������������ڽ�����";
            ErrorMsgMap[10038] = "����������һ���׽���";
            ErrorMsgMap[10039] = "Ŀ���ַ�Ǳ����";
            ErrorMsgMap[10040] = "���ݱ�̫���޷����뻺�����������ض�";
            ErrorMsgMap[10041] = "ָ���Ķ˿���Ϊ����׽��ִ�������";
            ErrorMsgMap[10042] = "��Ȩ��������֧�ֵ�";
            ErrorMsgMap[10043] = "ָ���Ķ˿��ǲ�֧��";
            ErrorMsgMap[10044] = "�׽������Ͳ�֧���ڴ˵�ַ��";
            ErrorMsgMap[10045] = " Socket�ǲ���һ�����ͣ���֧���������ӵķ���";
            ErrorMsgMap[10047] = "��ַ�岻֧��";
            ErrorMsgMap[10048] = "��ַ��ʹ����";
            ErrorMsgMap[10049] = "��ַ�ǲ��ǿ��Դӱ��ػ���";
            ErrorMsgMap[10050] = "������ϵͳʧ��";
            ErrorMsgMap[10051] = "������Դ�������������ʱ���ܴﵽ";
            ErrorMsgMap[10052] = "���ӳ�ʱ����SO_KEEPALIVEʱ";
            ErrorMsgMap[10053] = "���ӱ���ֹ�����ڳ�ʱ����������";
            ErrorMsgMap[10054] = "���ӱ��������ӱ�Զ�̶�����Զ�̶�";
            ErrorMsgMap[10055] = "�޻��������ÿռ�";
            ErrorMsgMap[10056] = "�׽���������";
            ErrorMsgMap[10057] = "�׽���δ����";
            ErrorMsgMap[10058] = "�׽����ѹر�";
            ErrorMsgMap[10060] = "�������ӳ�ʱ";
            ErrorMsgMap[10061] = "���ӱ�ǿ�ƾܾ�";
            ErrorMsgMap[10101] = "���������ѹر�";
            ErrorMsgMap[10201] = "�׽����Ѵ����˶���";
            ErrorMsgMap[10202] = "�׽�����δ�����˶���";
            ErrorMsgMap[11001] = "Ȩ���Ĵ𰸣��Ҳ�������";
            ErrorMsgMap[11002] = "��Ȩ���Ĵ𰸣��Ҳ�������";
            ErrorMsgMap[11003] = "�ǿɻָ��Ĵ���";
            ErrorMsgMap[11004] = "��Ч�����ƣ�û���������͵����ݼ�¼";
        }

        public static string DescrError(int ErrorCode)
        {
            if (ErrorMsgMap.Count == 0)
                SetErrorMsg();
            return ""+ ErrorMsgMap[ErrorCode];
        }


        public static bool HandleSocketError(SocketException socketExc)
        {
            bool handled = false;
            if (socketExc != null)
            {
                /**
                switch (socketExc.ErrorCode)
                {
                    case (int)WsaError.WSAEINTR:
                        LastError = string.Format("Socket call interrupted [code {0}].", socketExc.ErrorCode);
                        break;
                    case (int)WsaError.WSAEADDRINUSE:
                        LastError = string.Format("The address is already in use [code {0}].", socketExc.ErrorCode);
                        break;
                    case (int)WsaError.WSACONNABORTED:
                        LastError = string.Format("The connection was aborted [code {0}].", socketExc.ErrorCode);
                        break;
                    case (int)WsaError.WSAECONNRESET:
                        LastError = string.Format("Connection reset by peer [code {0}].", socketExc.ErrorCode);
                        break;
                    case (int)WsaError.WSAECONNREFUSED:
                        LastError = string.Format("The connection was refused by the remote host [code {0}].", socketExc.ErrorCode);
                        break;
                    case (int)WsaError.WSAEADDRNOTAVAIL:
                        LastError = string.Format("The requested address is not valid [code {0}].", socketExc.ErrorCode);
                        break;
                    default:
                        LastError = string.Format("Socket error [code {0}].", socketExc.ErrorCode);
                        break;
                }
                 */
                handled = true;
            }
            /**
            ObjectDisposedException disposedExc = socketExc as ObjectDisposedException;
            if (disposedExc != null)
            {
                LastError = "The socket has been closed.";
                handled = true;
            }

            if (LastError != string.Empty)
                Trace.Write(LastError);
            */
            //Trace.Write(exc.Message);
            //Trace.Write(exc.StackTrace);


            return handled;
        }
    }   // SockUtils
}
