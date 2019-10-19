using System;
using System.Data;
using LTE.DB;
using ESRI.ArcGIS.Geometry;
using LTE.InternalInterference.Grid;

namespace LTE.InternalInterference
{
	/// <summary>
	/// CJWDHelper ��ժҪ˵����
	/// ��֪��A �� ��B�ľ�γ�ȣ������ǵľ���͵�B����ڵ�A�ķ�λ
	///  ��֪��A��γ�ȣ�����B���A��ľ��룬�ͷ�λ����B��ľ�γ��
	/// </summary>
	public class CJWDHelper
	{
        public CJWDHelper()
        {
            //
            // TODO: �ڴ˴���ӹ��캯���߼�
            //
        }
        
		//! �����A �� ��B�ľ�γ�ȣ������ǵľ���͵�B����ڵ�A�ķ�λ
		/*! 
		  * \param A A�㾭γ��
		  * \param B B�㾭γ��
		  * \param angle B�����A�ķ�λ, ����Ҫ���ظ�ֵ��������Ϊ��
		  * \return A��B��ľ���
		  */
		public static double distance(JWD A, JWD B, ref double angle)
		{
			double dx = (B.m_RadLo - A.m_RadLo) * A.Ed;
			double dy = (B.m_RadLa - A.m_RadLa) * A.Ec;
			double outresult = Math.Sqrt(dx * dx + dy * dy);
  
			if( angle != 390)
			{
				if(dx==0.0)
					angle=0;
				else if(dy==0.0)
					angle=90;
				else
					angle = Math.Atan(Math.Abs(dx/dy))*180/Math.PI;
				// �ж�����
				double dLo = B.m_Longitude - A.m_Longitude;
				double dLa = B.m_Latitude - A.m_Latitude;
   
				if(dLo > 0 && dLa <= 0) 
				{
					angle = (90 - angle) + 90;
				}
				else if(dLo <= 0 && dLa < 0) 
				{
					angle = angle + 180;
				}
				else if(dLo < 0 && dLa >= 0) 
				{
					angle = (90 - angle) + 270;
				}
			}
			return outresult/1000;
		}
        /// <summary>
        /// ���������������
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns>����������룬��λǧ��</returns>
        public static double distance(JWD A, JWD B)
        {
            double dx = (B.m_RadLo - A.m_RadLo) * A.Ed;
            double dy = (B.m_RadLa - A.m_RadLa) * A.Ec;
            double outresult = Math.Sqrt(dx * dx + dy * dy);

            return outresult / 1000;
        }
		//! �����A �� ��B�ľ�γ�ȣ������ǵľ���͵�B����ڵ�A�ķ�λ
		/*! 
		  * \param longitude1 A�㾭��
		  * \param latitude1 A��γ��
		  * \param longitude2 B�㾭��
		  * \param latitude2 B��γ��
		  * \param angle B�����A�ķ�λ, ����Ҫ���ظ�ֵ��������Ϊ��
		  * \return A��B��ľ���
		  */
		public static double distance(
			double longitude1, double latitude1,
			double longitude2, double latitude2, 
			ref double angle)
		{
			JWD A=new JWD(longitude1,latitude1);
			JWD B=new JWD(longitude2,latitude2);
			return distance(A, B,ref angle);
		}


        public static double distance(
            double longitude1, double latitude1,
            double longitude2, double latitude2)
        {
            JWD A = new JWD(longitude1, latitude1);
            JWD B = new JWD(longitude2, latitude2);
            return distance(A, B);
        }
		//! ��֪��A��γ�ȣ�����B���A��ľ��룬�ͷ�λ����B��ľ�γ�ȣ� ����������0��
		/*!
		  * \param A ��֪��A
		  * \param distance B�㵽A��ľ��� 
		  * \param angle B�������A��ķ�λ
		  * \return B��ľ�γ������
		  */
		public static JWD GetJWDB(JWD A, double distance, double angle)
		{ 
			double dx = distance*1000 * Math.Sin(angle * Math.PI /180);
			double dy = distance*1000 * Math.Cos(angle * Math.PI /180);
  
			//double dx = (B.m_RadLo - A.m_RadLo) * A.Ed;
			//double dy = (B.m_RadLa - A.m_RadLa) * A.Ec;
			double BJD = (dx/A.Ed + A.m_RadLo) * 180/Math.PI;
			double BWD = (dy/A.Ec + A.m_RadLa) * 180/Math.PI;
			JWD B=new JWD(BJD, BWD);
			return B;
		}
		//! ��֪��A��γ�ȣ�����B���A��ľ��룬�ͷ�λ����B��ľ�γ��,��������Ϊ0��
		/*!
		  * \param longitude ��֪��A����
		  * \param latitude ��֪��Aγ��
		  * \param distance B�㵽A��ľ��� 
		  * \param angle B�������A��ķ�λ
		  * \return B��ľ�γ������
		  */
		public static JWD GetJWDB(double longitude, double latitude, double distance, double angle)
		{ 
			JWD A=new JWD(longitude,latitude);
			return GetJWDB(A, distance, angle);
		}
	
	}
}
