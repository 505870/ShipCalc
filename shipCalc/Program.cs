/********************************************
 * 
 * 
 *        과목 : 조선해양실무공학
 *        학과 : 조선해양공학
 *        담당 : 구남국 교수님
 *        학번 : 20113681
 *        이름 : 강지훈
 *        
 * 
 *********************************************/ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;


namespace shipCalc
{
    
    class Program
    {
        static double LBP;
        static int NumberOfWaterLine;
        static int NumberOfStation;
        static double[,] Offset;

        static double[] WaterPlaneArea;
        static double[] StationArea;
        static double[] Volume;

        static double[] VCB;
        static double[] LCB;
        static double[] TPC;
        static double[] LCF;
        static double[] KM;

        public static object[,] HydrostaticTable;


        static void Main(string[] args)
        {
            File_IO.ReadFile("Offset.csv");//데이터 파일 읽어오기

            LBP = File_IO.LBP;
            NumberOfWaterLine = File_IO.NumberOfWaterLine;
            NumberOfStation = File_IO.NumberOfStation;
            Offset = File_IO.Offset;

            WaterPlaneArea = new double[NumberOfWaterLine];
            StationArea = new double[NumberOfStation];
            Volume = new double[NumberOfWaterLine];
            VCB = new double[NumberOfWaterLine];
            LCB = new double[NumberOfWaterLine];
            TPC = new double[NumberOfWaterLine];
            LCF = new double[NumberOfWaterLine];
            KM = new double[NumberOfWaterLine];

            

            for (int i = 0; i < NumberOfWaterLine; i++)
            {
                double[] Line = GetLineFromOffset(Offset, i,1);
                WaterPlaneArea[i] = 2 * Simpson_Rule(Line, LBP / (NumberOfStation - 1));
                //Console.WriteLine("W.P Area {0} : {1}", File_IO.Row[i], WaterPlaneArea[i]);
            }

            for (int i = 0; i < NumberOfStation; i++)
            {
                double[] Line = GetLineFromOffset(Offset, i, 0);
                StationArea[i] = 2 * Simpson_Rule(Line, File_IO.Row[1]);
                //Console.WriteLine("ST Area {0} : {1}", i, StationArea[i]);
            }

            Volume[0] = 0;
            Volume[1] = (WaterPlaneArea[0] + WaterPlaneArea[1]) / 2;
            for (int i = 2; i < NumberOfWaterLine; i++)
            {
                double[] Line = new double[i + 1];
                for (int j = 0; j < i+1; j++)
                {
                    Line[j] = WaterPlaneArea[j];  
                    //Simpson 함수는 입력받은 배열의 심슨합을 출력하므로 WL별 계산시 Line[]을 재정의 해주어야 함.
                }
                Volume[i] = Simpson_Rule(Line, File_IO.Row[1],Line.Length);
                //Console.WriteLine("Volume {0} : {1}",File_IO.Row[i],Volume[i]);
            }

            HydrostaticTable = new object[NumberOfWaterLine - 1 , 8]; //출력용 테이블 3워터라인부터 출력하므로 2 제외
        
            for(int i = 2; i<NumberOfWaterLine; i++)  //2개 이하는 심슨룰이 되지 않는다.
            {

                VCB[i]=Calc_VCB(i);
                //Console.WriteLine("VCB {0} : {1}", i,VCB[i]);
                
                LCB[i] = Calc_LCB(i);
                //Console.WriteLine("LCB {0} : {1}", i, LCB[i]);

                TPC[i] = Calc_TPC(i);
                //Console.WriteLine("TPC {0} : {1}", i,TPC[i]);
                
                LCF[i] = Calc_LCF(i);
                //Console.WriteLine("LCF {0} : {1}", i, LCF[i]);

                KM[i] = Calc_BM(i) + VCB[i];  //VCB 계산 후 수행해야 함.
                //Console.WriteLine("KM {0} : {1}", i, KM[i]);

                HydrostaticTable[i - 1, 1] = Volume[i];
                HydrostaticTable[i - 1, 2] = VCB[i];
                HydrostaticTable[i - 1, 3] = LCB[i];
                HydrostaticTable[i - 1, 4] = LCF[i];
                HydrostaticTable[i - 1, 5] = KM[i];
                HydrostaticTable[i - 1, 6] = TPC[i];
                HydrostaticTable[i - 1, 7] = WaterPlaneArea[i];

                
            }

            HydrostaticTable[0, 0] = "Draft(M)";
            HydrostaticTable[0, 1] = "Disp(M3)";
            HydrostaticTable[0, 2] = "VCB(M)";
            HydrostaticTable[0, 3] = "LCB(M)";
            HydrostaticTable[0, 4] = "LCF(M)";
            HydrostaticTable[0, 5] = "KM(M3)";
            HydrostaticTable[0, 6] = "TPC(Ton)";
            HydrostaticTable[0, 7] = "W.P Area(M2)";

            HydrostaticTable = File_IO.Swap(HydrostaticTable);

            File_IO.FileOut("Hydrostatic.csv");

            
        }//main 끝


        //static void Simsion_Rule(double[,] input, int num, double h, int row)    //2차원 배열을 입력받아 1차원배열로 출력
        //{
            
        //    Tmp_Area= new double[input.GetLength(0)];
            

        //    if (num % 2 == 1)                              // 적절하게 홀수개의 종선 가졌다면(1st Rule)
        //    {
        //        for (int i = 0; i < input.GetLength(0); i++)           //NumberOfWaterLine
        //        {
        //            Tmp_Area[i] = 0;
        //            for (int j = 0; j < num-1; j += 2)          
        //            {
        //                Tmp_Area[i] += h/3 * (input[i, j] + 4 * input[i, j + 1] + input[i, j + 2]);
        //                //Console.WriteLine("{0}, {1}, {2}", input[i, j], 4 * input[i, j + 1], input[i, j + 2]);
        //            }
        //        }
        //        for (int i = 0; i < row; i++)
        //        {
        //            Console.WriteLine("Simsion(1ST RULE)[{0}]={1}", i, Tmp_Area[i]);      //Simsion 1st Rule 결과 값 출력
        //        }
        //    }

        //    else if (num % 2 == 0)                          //짝수개의 종선(1st Rule+2nd Rule)
        //    {
        //        if (num == 4)                               //특수한 경우 (종선갯수=4)
        //        {
        //            for (int i = 0; i < row ; i++)
        //            {
        //                Tmp_Area[i] = 0;
        //                Tmp_Area[i] += 3*h/8 * (input[i, 0] + 3 * input[i, 1] + 3 * input[i, 2] + input[i, 3]);
        //                //Console.WriteLine("{0}", h*( input[i, 0] + 3 * input[i, 1] + 3 * input[i, 2] + input[i, 3]));
        //            }
        //            for (int i = 0; i < input.GetLength(0); i++)
        //            {
        //                Console.WriteLine("Simsion(2ND RULE)[{0}]={1}", i, Tmp_Area[i]);      //Simsion 2nd Rule 결과 값 출력
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 0; i < row; i++)            //NumberOfWaterLine
        //            {
        //                Tmp_Area[i] = 0;
        //                for (int j = 0; j < num - 4; j += 2)
        //                {
        //                    Tmp_Area[i] += h / 3 * (input[i, j] + 4 * input[i, j + 1] + input[i, j + 2]);
        //                    //Console.WriteLine("{0}, {1}, {2}", input[i, j], 4 * input[i, j + 1], input[i, j + 2]);
        //                }
        //                //서비스 넣어주기(2nd Rule)
        //                Tmp_Area[i] += 3 * h / 8 * (input[i, num - 4] + 3 * input[i, num - 3] + 3 * input[i, num - 2] + input[i, num-1]);
        //                //Console.WriteLine(i);
        //            }
        //            for (int i = 0; i < input.GetLength(0); i++)
        //            {
        //                Console.WriteLine("Simsion(1ST+2ND RULE)[{0}]={1}", i, Tmp_Area[i]);      //Simsion 2nd Rule 결과 값 출력
        //            }
        //        }
                
        //    }
        //    else
        //    {
        //        Console.WriteLine("Failed");
        //    }
        // }

        static double[] Moment(double[] _DataArr, double[] _LeverArr)
        {
            double[] Result = new double[_DataArr.Length];
            for (int i = 0; i < _DataArr.Length; i++ )
            {
                Result[i] = _DataArr[i] * _LeverArr[i];
            }
            return Result;
        }


        static double[] GetLineFromOffset(double[,] _Offset, int _Number, int _01)
        {
            double[] Line = new double[_Offset.GetLength(_01)];
            for (int i = 0; i < _Offset.GetLength(_01); i++)
            {
                if(_01 == 1)
                {
                    Line[i] = _Offset[_Number, i];
                }
                else if (_01 == 0)
                {
                    Line[i] = _Offset[i, _Number];
                }
            }
            return Line;
        }


        static double Simpson_Rule(double[] _Arr, double _Interval, int _Num_of_Point)
        {
            double Sum = 0;

            if (_Num_of_Point % 2 == 1)                              // 적절하게 홀수개의 종선 가졌다면(1st Rule)
            {
                for (int i = 0; i < _Num_of_Point; i++)
                {
                    if (i == 0 || i == _Num_of_Point - 1) { Sum = Sum + _Arr[i]; }
                    else if (i % 2 == 1) { Sum = Sum + 4 * _Arr[i]; }
                    else if (i % 2 == 0) { Sum = Sum + 2 * _Arr[i]; }
                }
                return _Interval / 3 * Sum;
            }
            else if (_Num_of_Point % 2 == 0)                          //짝수개의 종선(1st Rule+2nd Rule)
            {
                if (_Num_of_Point == 4)                               //특수한 경우 (종선갯수=4)
                {
                    Sum = _Interval * 3 / 8 * (_Arr[0] + 3 * _Arr[1] + 3 * _Arr[2] + _Arr[3]);  //return 시 상수를 앞에쓰면 0 반환.
                    return Sum;
                }
                else
                {
                    for (int i = 0; i < _Num_of_Point - 3; i++)
                    {
                        if (i == 0 || i == _Num_of_Point - 4) { Sum = Sum + _Arr[i]; }
                        else if (i % 2 == 1) { Sum = Sum + 4 * _Arr[i]; }
                        else if (i % 2 == 0) { Sum = Sum + 2 * _Arr[i]; }
                    }
                    Sum = _Interval / 3 * Sum;
                    Sum = Sum + _Interval * 3 / 8 * (_Arr[_Num_of_Point - 4] + 3 * _Arr[_Num_of_Point - 3] + 3 * _Arr[_Num_of_Point - 2] + _Arr[_Num_of_Point - 1]);
                    return  Sum;
                }
            }
            else
            {
                Console.WriteLine("Failed. Check input Value");
                return 0;
            }
        }  //심슨 적분 끝
        static double Simpson_Rule(double[] _Arr, double _Interval)
        {
            double Sum = 0;

            if (_Arr.Length % 2 == 1)                              // 적절하게 홀수개의 종선 가졌다면(1st Rule)
            {
                for (int i = 0; i < _Arr.Length; i++)
                {
                    if (i == 0 || i == _Arr.Length - 1) { Sum = Sum + _Arr[i]; }
                    else if (i % 2 == 1) { Sum = Sum + 4 * _Arr[i]; }
                    else if (i % 2 == 0) { Sum = Sum + 2 * _Arr[i]; }
                }
                return _Interval / 3 * Sum;
            }
            else if (_Arr.Length % 2 == 0)                          //짝수개의 종선(1st Rule+2nd Rule)
            {
                if (_Arr.Length == 4)                               //특수한 경우 (종선갯수=4)
                {
                    Sum = _Interval * 3 / 8 * (_Arr[0] + 3 * _Arr[1] + 3 * _Arr[2] + _Arr[3]);  //return 시 상수를 앞에쓰면 0 반환.
                    return Sum;
                }
                else
                {
                    for (int i = 0; i < _Arr.Length - 3; i++)
                    {
                        if (i == 0 || i == _Arr.Length - 4) { Sum = Sum + _Arr[i]; }
                        else if (i % 2 == 1) { Sum = Sum + 4 * _Arr[i]; }
                        else if (i % 2 == 0) { Sum = Sum + 2 * _Arr[i]; }
                    }
                    Sum = _Interval / 3 * Sum;
                    Sum = Sum + _Interval * 3 / 8 * (_Arr[_Arr.Length - 4] + 3 * _Arr[_Arr.Length - 3] + 3 * _Arr[_Arr.Length - 2] + _Arr[_Arr.Length - 1]);
                    return Sum;
                }
            }
            else
            {
                Console.WriteLine("Failed. Check input Value");
                return 0;
            }
        }  //심슨 적분 끝


        static double[] Calc_Lever(double _startingValue, double _Gap, int _Num)
        {
            double[] Result = new double[_Num];
            if (_startingValue == 0)  //**********끝단에서의 모멘트 암
            {
                for (int i = 0; i < _Num; i++)  
                {
                    Result[i] = _startingValue + i * _Gap; //등차수열 일반항
                }
                return Result;
            }
            else  //***************************** 특정 지점에 대한 모멘트암
            {
                for (int i = 0; i < _Num; i++)  //끝단에서의 모멘트 암
                {
                    Result[i] = i * _Gap - _startingValue; //등차수열 일반항
                }
                return Result;
            }
        }



        static double Calc_TPC(int _WL) 
        {
            double TPC;
            TPC = WaterPlaneArea[_WL] * 1.025 / 100;
            return TPC;
        }
       
        static double Calc_VCB(int _WL)
        {
            double[] Lever = new double [NumberOfWaterLine];
            Lever = Calc_Lever(0, File_IO.Row[1], NumberOfWaterLine);
            double[] Result = Moment(WaterPlaneArea, Lever);
            double VCB = Simpson_Rule(Result, File_IO.Row[1], _WL + 1) / Volume[_WL];
            return VCB;
        }

        static double Calc_LCB(int _WL)
        {
            double[] StationArea_WL = new double[NumberOfStation];
            for (int i = 0; i < NumberOfStation; i++)
            {
                double[] Line = GetLineFromOffset(Offset, i, 0);
                StationArea_WL[i] = 2 * Simpson_Rule(Line, File_IO.Row[1], _WL + 1);
            }
            double[] Lever = new double[NumberOfStation];
            Lever = Calc_Lever(0, LBP / (NumberOfStation - 1), NumberOfStation);
            double[] Result = Moment(StationArea_WL, Lever);
            double LCB = Simpson_Rule(Result, LBP / (NumberOfStation - 1), NumberOfStation) / Volume[_WL];
            return LCB;
        }

        static double Calc_LCF(int _WL)
        {
            double[] Lever = new double[NumberOfStation];
            Lever = Calc_Lever(0, LBP / (NumberOfStation - 1), NumberOfStation);
            double[] Line = GetLineFromOffset(Offset, _WL, 1);  //WL에 따라 오프셋 값을 다르게 읽을 예정. -> 심슨룰시 NumberOfStation 써도 무방.
            for (int i =0; i< Line.Length;i++)  //반폭을 전폭으로 확장
            {
                Line[i] = 2 * Line[i];
            }
            double[] Result = Moment(Line,Lever);
            double LCF = Simpson_Rule(Result, LBP / (NumberOfStation - 1), NumberOfStation) / WaterPlaneArea[_WL];
            return LCF;
        }

        static double Calc_BM(int _WL)
        {
            //double[] Lever = new double[NumberOfStation]; //모멘트 암
            double[] Half_Breadth_POW3 = new double[NumberOfStation]; //반폭의 3승

            //Lever = Calc_Lever(LBP / 2, LBP / (NumberOfStation - 1), NumberOfStation);  
            double[] Helf_Breadth = GetLineFromOffset(Offset, _WL, 1);  //WL에 따라 오프셋 값을 다르게 읽을 예정. -> 심슨룰시 NumberOfStation 써도 무방.
            for (int i = 0; i < NumberOfStation; i++)  //반폭을 전폭으로 확장 + 반폭 3승(수선면 관성모멘트 유도용)
            {
                Half_Breadth_POW3[i] = Math.Pow(Helf_Breadth[i], 3); //System.Math 내장 Method.
            }
            double BM = 2 * Simpson_Rule(Half_Breadth_POW3, LBP / (NumberOfStation - 1), NumberOfStation) / (3 * Volume[_WL]);
            return BM;
        }


        //내가 왜 삽질만 하는지 자괴감들고 괴로워...
        } //class 끝








    class File_IO
    {
        public static double LBP;
        public static int NumberOfWaterLine;
        public static int NumberOfStation;

        public static double[,] Offset;   //2차원 배열 선언

        static string Buff;
        static string[] tokens;
        static bool Row_WL = true;  //가로축에 워터라인 값을 입력받는 것을 default 값으로 한다.
        public static List<double> Row = new List<double>();

        public static string[] Split_CSV(string input)
        {
            Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)");
            List<string> list = new List<string>();
            string curr = null;
            foreach (Match match in csvSplit.Matches(input))
            {
                curr = match.Value;
                if (0 == curr.Length)
                {
                    list.Add("");
                }
                list.Add(curr.TrimStart(','));
            }
            for (int i = 0; i < list.Count(); i++)
            {
                list[i] = Regex.Replace(list[i], "[,]+\"|\"[,]+", "");  //따옴표 및 콤마 제거
                list[i] = Regex.Replace(list[i], "\"", ""); //콤마만 제거
                list[i] = Regex.Replace(list[i], ",", "."); //오타난 콤마 치환
                list[i] = Regex.Replace(list[i], @"\.\.", "");
            }
            return list.ToArray<string>();
        }

        public static double[,] Swap(double[,] _Arr) //offset 스왑용
        {
            double[,] Reverse = new double[_Arr.GetLength(1), _Arr.GetLength(0)];
            for (int j = 0; j<_Arr.GetLength(1); j++)
            {
                for (int i = 0; i < _Arr.GetLength(0); i++)
                {
                    Reverse[j, i] = _Arr[i, j];
                }
            }
            return Reverse;
        }
       
        public static object[,] Swap(object[,] _Arr)  //하이드로테이블 스왑용
        {
            object[,] Reverse = new object[_Arr.GetLength(1), _Arr.GetLength(0)];
            for (int j = 0; j < _Arr.GetLength(1); j++)
            {
                for (int i = 0; i < _Arr.GetLength(0); i++)
                {
                    Reverse[j, i] = _Arr[i, j];
                }
            }
            return Reverse;
        }

        public static void ReadFile(string _fileName)
        {
            StreamReader sr = new StreamReader(new FileStream(_fileName, FileMode.Open));

            Console.WriteLine("CSV Parsing Start!");
            do
            {
                Buff = sr.ReadLine();
                if (Buff == null) { break; }   // 모든 줄을 다 읽을을때 까지 한줄씩 읽어오기(다 읽으면 null 반환됨.)

                if (Buff.StartsWith(",") == true || Buff == "")  //만약 내용 없는 줄(콤마로 시작함)이 오면 다음 줄을 읽는다.(notepad에서 편집하면 콤마로 시작하진 않음)
                {
                    //Console.WriteLine("-");
                }
                else  //내용있는 줄이 오면...
                {
                    tokens = Split_CSV(Buff);
                    //tokens = Buff.Split(" ,\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    
                    switch (tokens[0])  //분기제어를 이용한 사용자 입력값 가져오기.
                    {
                        case "LBP":
                        case "Lbp":
                        case "lbp":
                        case "LPP":
                        case "Lpp":
                        case "lpp":
                        case "Length Between Perpendiculars":
                        case "length between perpendiculars":
                            LBP = double.Parse(tokens[1]);
                            Console.WriteLine("LBP = {0}", LBP);;
                            break;
                        case "Offset":
                        case "offset":
                            tokens = Split_CSV(Buff);
                            //*****************************************가로열 오프셋 저장

                            if (Regex.IsMatch(Buff, @"(WL)|(wl)|(Wl)") && Regex.IsMatch(Buff, @"(ST)|(st)|(St)") == false)
                            {
                                NumberOfWaterLine = tokens.Length - 1 ; //워터라인 갯수 구하기
                                Console.WriteLine("Number Of WaterLine : {0}", NumberOfWaterLine);
                            }
                            else if (Regex.IsMatch(Buff, @"(ST)|(st)|(St)") && Regex.IsMatch(Buff, @"(WL)|(wl)|(Wl)") == false)
                            {
                                NumberOfStation = tokens.Length - 1; //스테이션 갯수 구하기
                                Console.WriteLine("Number Of Station : {0}", NumberOfStation);
                                Row_WL = false;
                            }
                            else
                            {
                                Console.WriteLine("Invalid 0ffset value. Check the manual.");
                            }
                            for (int i = 0; i < tokens.Length; i++) //오프셋 가로열 위치만 읽기
                            {
                                tokens[i] = Regex.Replace(tokens[i], @"[^0-9^\.]", "");  //숫자 및 포인트 빼고 제거
                                if ( i > 0) // offset 내용을 담은 토큰이라면
                                {
                                    if (i == 1)  
                                    {
                                        Row.Add(0); // BaseLine 라면 B.LWL 대신 0 넣자
                                    }
                                    else
                                    {
                                        Row.Add(double.Parse(tokens[i]));
                                    }
                                }
                            }  //가로행 값 리스트 저장 끝

                            List<string> Column = new List<string>();//*****************************************세로열 오프셋 저장
                            while(true)  //세로열 갯수 세기
                            {
                                int i = 0;
                            R1:
                                Buff = sr.ReadLine();
                                if (Buff == null || Buff.StartsWith(",") == true || Buff == "") { break; } 
                                //버퍼 양식이 offset에 안맞으면 저장을 종료한다.
                                Column.Add(Buff);
                                i++;
                                goto R1;
                            }

                            if (Row_WL == true)
                            {
                                NumberOfStation = Column.Count();
                                Console.WriteLine("Number Of Station : {0}", NumberOfStation);
                            }
                            else
                            {
                                NumberOfWaterLine = Column.Count();
                                Console.WriteLine("Number Of WaterLine : {0}", NumberOfWaterLine);
                            }
                            Offset = new double[NumberOfWaterLine, NumberOfStation];  //offset 저장 배열(가로열 WL을 기준으로 함)

                            for (int j = 0; j < Column.Count(); j++)
                            {
                                tokens = Split_CSV(Column[j]);

                                //tokens = Column[j].Split(" ,\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < Row.Count(); i++)
                                {
                                    Offset[i, j] = double.Parse(tokens[i + 1]);
                                }
                            }

                            if (Row_WL == false) { Offset = Swap(Offset); }  //가로열이 스테이션 일 경우 Swap하기


                            break;
                        default:
                            Console.WriteLine("{0} is an invalid 0ffset value. Check the manual.", tokens[0]);
                            break;
                    }

                }

            } while (true);  // 모든 줄을 다 읽을을때 까지 한줄씩 읽어오기
            
            sr.Close();
        }


        public static void FileOut(string _Filename)
        {
            StreamWriter sw = new StreamWriter(new FileStream(_Filename, FileMode.Create));

            for (int i = 0; i < 13; i++)
            {
                if (i == 1 || i == 2) { continue; } //wl=2~3 스킵

                if (i > 0) { Program.HydrostaticTable[0, i] = i + 1; }

                for (int text = 0; text < Program.HydrostaticTable.GetLength(0); text++)
                {
                    sw.Write("{0},", Program.HydrostaticTable[text,i]);
                }
                sw.Write("\n");
            }
            sw.Close();
        }//File Writing 끝
    }//class_IO 끝
        

}//name space 끝



    

