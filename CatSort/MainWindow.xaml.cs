using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace CatSort {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        DispatcherTimer dispatcherTimer;
        Timer timer;
        DataProcess dataProcess = new DataProcess();
        uint dataLenInput = 20;
        static int pixySize = 4;
        static uint maxTimes = 0;
        static bool flagWinAdj = false;
        static ulong numProgress = 0, maxNumProgress = 0;
        static uint numDynamicProgress = 0;
        static uint time500ms = 0;

        public MainWindow() {
            InitializeComponent();
            TimerInit();
            //Draw();
        }
        public void TimerInit() {
            TimerCallback timeCB = new TimerCallback(Timer_Tick);
            timer = new Timer(timeCB, null, 0, 50);
        }
        private void Timer_Tick(object obj) {
            time500ms += 1;
            Dispatcher.Invoke(
                new Action(
                    delegate {
                        if (tabControl.SelectedIndex == 0) {
                            dataProgress.Value = maxNumProgress > 0 ? numProgress * 100 / maxNumProgress : 0;
                        }
                        else {
                            if (time500ms >= 10) {
                                if (numDynamicProgress < maxTimes) {
                                    DrawDynamic(numDynamicProgress);
                                    numDynamicProgress += 1;
                                }
                                else {

                                }
                                dataProgress.Value = maxTimes > 0 ? numDynamicProgress * 100 / maxTimes : 0;
                                time500ms = 0;
                            }
                        }
                        
                        if (Show7.IsMeasureValid && flagWinAdj) {
                            if (windowChange.IsChecked == true) {
                                WindowAdj();
                            }
                            flagWinAdj = false;
                        }
                    }));

        }
        public void DoEvents() {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrames), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrames(object f) {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
        public void DrawRow(Canvas obj, double x, List<uint> listRow) {
            //入参xy为在打点矩阵中的第x行的点，不是实际坐标
            Color color = new Color();
            SolidColorBrush solidColorBrush = new SolidColorBrush(color);
            for (int i = 0; i < listRow.Count; i++) {
                solidColorBrush.Color = DataProcess.ColorConversionHelper.GreytoRGB((byte)listRow[i]);
                Rectangle rectangle = new Rectangle {
                    Fill = new SolidColorBrush(DataProcess.ColorConversionHelper.GreytoRGB((byte)listRow[i])),
                    Margin = new Thickness(i * pixySize, x * pixySize + 35, 0, 0),
                    Height = pixySize,
                    Width = pixySize
                };
                obj.Children.Add(rectangle);
                
            }
            //DoEvents();
        }
        public void DrawPixy(Canvas obj, double x, double y, byte brightness, int size) {
            //入参xy为在打点矩阵中的第x行y列的点，不是实际坐标
            Color color = new Color();
            color = DataProcess.ColorConversionHelper.GreytoRGB(brightness);
            Brush brush = new SolidColorBrush(color);
            Rectangle rectangle = new Rectangle {
                Fill = brush,
                Margin = new Thickness(x * size, 35 + y * size, 0, 0),
                Height = size,
                Width = size
            };
            obj.Children.Add(rectangle);
            
        }
        public void DrawDynamic(uint x) {
            if (x <= dataProcess.timesBubble) { DrawRow(DShow1, 0, dataProcess.lstBubbleProcess[(int)x]); }
            if (x <= dataProcess.timesSelection) { DrawRow(DShow2, 0, dataProcess.lstSelectionProcess[(int)x]); }
            if (x <= dataProcess.timesInsertion) { DrawRow(DShow3, 0, dataProcess.lstInsertionProcess[(int)x]); }
            if (x <= dataProcess.timesShell) { DrawRow(DShow4, 0, dataProcess.lstShellProcess[(int)x]); }
            if (x <= dataProcess.timesMerge) { DrawRow(DShow5, 0, dataProcess.lstMergeProcess[(int)x]); }
            if (x <= dataProcess.timesHeap) { DrawRow(DShow6, 0, dataProcess.lstHeapProcess[(int)x]); }
            if (x <= dataProcess.timesQuick) { DrawRow(DShow7, 0, dataProcess.lstQuickProcess[(int)x]); }
            DtbBubbleTime.Text = (x > dataProcess.timesBubble ? dataProcess.timesBubble : x).ToString();
            DtbSelectionTime.Text = (x > dataProcess.timesSelection ? dataProcess.timesSelection : x).ToString();
            DtbInsertionTime.Text = (x > dataProcess.timesInsertion ? dataProcess.timesInsertion : x).ToString();
            DtbShellTime.Text = (x > dataProcess.timesShell ? dataProcess.timesShell : x).ToString();
            DtbMergeTime.Text = (x > dataProcess.timesMerge ? dataProcess.timesMerge : x).ToString();
            DtbHeapTime.Text = (x > dataProcess.timesHeap ? dataProcess.timesHeap : x).ToString();
            DtbQuickTime.Text = (x > dataProcess.timesQuick ? dataProcess.timesQuick : x).ToString();
            //numProgress = x;
        }
        public void DataCal() {
            numProgress = 0;
            maxNumProgress = 0;
            numDynamicProgress=0;
            //设置数据长度
            dataProcess.SetDataLen(dataLenInput);

            dataProcess.BubbleSort();
            dataProcess.SelectionSort();
            dataProcess.InsertionSort();
            dataProcess.ShellSort();
            dataProcess.MergeSort();
            dataProcess.HeapSort();
            dataProcess.QuickSort();

            maxNumProgress += dataProcess.timesBubble + 1;
            maxNumProgress += dataProcess.timesSelection + 1;
            maxNumProgress += dataProcess.timesInsertion + 1;
            maxNumProgress += dataProcess.timesShell + 1;
            maxNumProgress += dataProcess.timesMerge + 1;
            maxNumProgress += dataProcess.timesHeap + 1;
            maxNumProgress += dataProcess.timesQuick + 1;


            List<uint> maxTemp = new List<uint>(7) {
                dataProcess.timesBubble,
                dataProcess.timesHeap,
                dataProcess.timesInsertion,
                dataProcess.timesMerge,
                dataProcess.timesQuick,
                dataProcess.timesSelection,
                dataProcess.timesShell
            };
            maxTimes = maxTemp.Max();
            tbBubbleTime.Text = dataProcess.timesBubble.ToString();
            tbHeapTime.Text = dataProcess.timesHeap.ToString();
            tbInsertionTime.Text = dataProcess.timesInsertion.ToString();
            tbMergeTime.Text = dataProcess.timesMerge.ToString();
            tbQuickTime.Text = dataProcess.timesQuick.ToString();
            tbSelectionTime.Text = dataProcess.timesSelection.ToString();
            tbShellTime.Text = dataProcess.timesShell.ToString();
        }
        public void ClearDynamic() {
            DShow1.Children.RemoveRange(2, DShow1.Children.Count);
            DShow2.Children.RemoveRange(2, DShow2.Children.Count);
            DShow3.Children.RemoveRange(2, DShow3.Children.Count);
            DShow4.Children.RemoveRange(2, DShow4.Children.Count);
            DShow5.Children.RemoveRange(2, DShow5.Children.Count);
            DShow6.Children.RemoveRange(2, DShow6.Children.Count);
            DShow7.Children.RemoveRange(2, DShow7.Children.Count);
            dataProgress.Value = 0;
        }
        public void Draw() {
            //清空当前界面
            dataProgress.Value = 0;
            while (Show1.Children.Count>2) {
                Show1.Children.RemoveRange(2, Show1.Children.Count - 2 > 5000 ? 5000 : Show1.Children.Count - 2);
                DoEvents();
            }
            while (Show2.Children.Count > 2) {
                Show2.Children.RemoveRange(2, Show2.Children.Count - 2 > 5000 ? 5000 : Show2.Children.Count - 2);
                DoEvents();
            }
            while (Show3.Children.Count > 2) {
                Show3.Children.RemoveRange(2, Show3.Children.Count - 2 > 5000 ? 5000 : Show3.Children.Count - 2);
                DoEvents();
            }
            while (Show4.Children.Count > 2) {
                Show4.Children.RemoveRange(2, Show4.Children.Count - 2 > 5000 ? 5000 : Show4.Children.Count - 2);
                DoEvents();
            }
            while (Show5.Children.Count > 2) {
                Show5.Children.RemoveRange(2, Show5.Children.Count - 2 > 5000 ? 5000 : Show5.Children.Count - 2);
                DoEvents();
            }
            while (Show6.Children.Count > 2) {
                Show6.Children.RemoveRange(2, Show6.Children.Count - 2 > 5000 ? 5000 : Show6.Children.Count - 2);
                DoEvents();
            }
            while (Show7.Children.Count > 2) {
                Show7.Children.RemoveRange(2, Show7.Children.Count - 2 > 5000 ? 5000 : Show7.Children.Count - 2);
                DoEvents();
            }
            
            
            
            flagWinAdj = true;
            WidgetAdj();
            DoEvents();
            //display
            {
                //显示bubble sort
                for (int j = 0; j < dataProcess.timesBubble + 1; j++) {
                    DrawRow(Show1, j, dataProcess.lstBubbleProcess[j]);
                    numProgress += 1;
                    if (j % 10 == 0) {
                        DoEvents();
                    }
                }
                //显示selection sort
                for (int j = 0; j < dataProcess.timesSelection + 1; j++) {
                    DrawRow(Show2, j, dataProcess.lstSelectionProcess[j]);
                    numProgress += 1;
                    if (j % 10 == 0) {
                        DoEvents();
                    }
                }
                //显示insertion sort
                for (int j = 0; j < dataProcess.timesInsertion + 1; j++) {
                    DrawRow(Show3, j, dataProcess.lstInsertionProcess[j]);
                    numProgress += 1;
                    if (j % 10 == 0) {
                        DoEvents();
                    }
                }
                //显示shell sort
                for (int j = 0; j < dataProcess.timesShell + 1; j++) {
                    DrawRow(Show4, j, dataProcess.lstShellProcess[j]);
                    numProgress += 1;
                    if (j % 10 == 0) {
                        DoEvents();
                    }
                }
                //显示merge sort
                for (int j = 0; j < dataProcess.timesMerge + 1; j++) {
                    DrawRow(Show5, j, dataProcess.lstMergeProcess[j]);
                    numProgress += 1;
                    if (j % 10 == 0) {
                        DoEvents();
                    }
                }
                //显示heap sort
                for (int j = 0; j < dataProcess.timesHeap + 1; j++) {
                    DrawRow(Show6, j, dataProcess.lstHeapProcess[j]);
                    numProgress += 1;
                    if (j % 10 == 0) {
                        DoEvents();
                    }
                }
                //显示quick sort
                for (int j = 0; j < dataProcess.timesQuick + 1; j++) {
                    DrawRow(Show7, j, dataProcess.lstQuickProcess[j]);
                    numProgress += 1;
                    if (j % 10 == 0) {
                        DoEvents();
                    }
                }
            }

        }
        public void WindowAdj() {
            if (GetWindow(this).WindowState == WindowState.Maximized) {

            }
            else if(GetWindow(this).WindowState == WindowState.Normal){
                if((GetWindow(this).Width<1000)&&(GetWindow(this).Width < ((dataLenInput * pixySize + 20) * 7+50))) {
                    GetWindow(this).Width= ((dataLenInput * pixySize + 20) * 7 + 100) <1000? ((dataLenInput * pixySize + 20) * 7 + 100) :1000;
                }
                if ((GetWindow(this).Height < 700) && (GetWindow(this).Height < ((maxTimes * pixySize + 10) * 7 + 40))) {
                    GetWindow(this).Height = ((maxTimes * pixySize + 10) * 7 + 80)<700? ((maxTimes * pixySize + 10) * 7 + 80):700;
                }
                
            }
            else {

            }
        }
        public void WidgetAdj() {
            MainBoard.Width = (dataLenInput * pixySize + 20) * MainBoard.Columns;
            MainBoard.Height = maxTimes * pixySize + 80;
            if (MainBoard.Width < 300) { MainBoard.Width = 300; }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            dataLenInput = Convert.ToUInt32(dataLenFront.Text);
            if (dataLenInput > 256) {
                dataLenInput = 256;
            }
            if (dataLenInput < 2) {
                dataLenInput = 2;
            }

            if (dataLenInput >= 150) {
                pixySize = 1;
            }
            else if (dataLenInput >= 90) {
                pixySize = 2;
            }
            else if (dataLenInput >= 50) {
                pixySize = 3;
            }
            else if (dataLenInput >= 30) {
                pixySize = 4;
            }
            else if (dataLenInput >= 20) {
                pixySize = 5;
            }
            else if (dataLenInput >= 10) {
                pixySize = 6;
            }
            else{
                pixySize = 7;
            }
            DataCal();
            if (tabControl.SelectedIndex == 0) {
                Draw();
                
                WidgetAdj();
            }
            else {
                ClearDynamic();
                DrawDynamic(0);
            }
            
        }

        private void dataLenFront_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            Regex re = new Regex("[^0-9]+");
            e.Handled = re.IsMatch(e.Text);
        }
    }
    public class DataProcess {
        public uint dataLen = 0;
        public List<uint> lstOrigin = new List<uint>();
        public uint dataRatio = 0;
        //bubble
        public uint timesBubble = 0;
        public List<List<uint>> lstBubbleProcess = new List<List<uint>>();
        //selection
        public uint timesSelection = 0;
        public List<List<uint>> lstSelectionProcess = new List<List<uint>>();
        //insertion
        public uint timesInsertion = 0;
        public List<List<uint>> lstInsertionProcess = new List<List<uint>>();
        //Shell
        public uint timesShell = 0;
        public List<List<uint>> lstShellProcess = new List<List<uint>>();
        //Merge
        public uint timesMerge = 0;
        public List<List<uint>> lstMergeProcess = new List<List<uint>>();
        //heap
        public uint timesHeap = 0;
        public List<List<uint>> lstHeapProcess = new List<List<uint>>();
        //quick
        public uint timesQuick = 0;
        public List<List<uint>> lstQuickProcess = new List<List<uint>>();


        public void SetDataLen(uint len) {
            Random random = new Random();
            uint temp;
            uint cont = 0;
            //List<uint> listTemp =new List<uint> { 20, 19, 18,17,16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1 };

            dataLen = 0;
            dataRatio = 0;
            lstOrigin.Clear();

            timesBubble = 0;
            lstBubbleProcess.Clear();
            timesSelection = 0;
            lstSelectionProcess.Clear();
            timesInsertion = 0;
            lstInsertionProcess.Clear();
            timesShell = 0;
            lstShellProcess.Clear();
            timesMerge = 0;
            lstMergeProcess.Clear();
            timesHeap = 0;
            lstHeapProcess.Clear();
            timesQuick = 0;
            lstQuickProcess.Clear();

            if (len <= 256) { dataLen = len; if (len < 2) { dataLen = 2; } }
            else { dataLen = 256; }
            //根据数据长度设置显示分辨率
            dataRatio = 256 / dataLen;
            //生成随机数放入list
            while (cont < dataLen) {
                //temp = dataRatio * (uint)random.Next(-1, (int)dataLen);
                temp = dataRatio * (uint)random.Next(0, (int)dataLen);
                if (!lstOrigin.Contains(temp)) {
                    lstOrigin.Add(temp);
                    cont += 1;
                }
            }
            //lstOrigin.AddRange(listTemp);

        }
        public void BubbleSort() {
            List<uint> lstBubble = new List<uint>();
            lstBubbleProcess.Clear();
            lstBubble.AddRange(lstOrigin);
            lstBubbleProcess.Add(Clone<uint>(lstBubble));
            for (int i = 0; i < dataLen - 1; i++) {
                for (int j = 0; j < dataLen - 1 - i; j++) {
                    if (lstBubble[j] > lstBubble[j + 1]) {        // 相邻元素两两对比
                        uint temp = lstBubble[j + 1];        // 元素交换
                        lstBubble[j + 1] = lstBubble[j];
                        lstBubble[j] = temp;
                        //记录每一次操作
                        lstBubbleProcess.Add(Clone<uint>(lstBubble));
                        timesBubble += 1;
                    }
                }

            }
        }
        public void SelectionSort() {
            List<uint> lstSelection = new List<uint>();
            lstSelectionProcess.Clear();
            lstSelection.AddRange(lstOrigin);
            lstSelectionProcess.Add(Clone<uint>(lstSelection));

            int minIndex; uint temp;
            for (int i = 0; i < dataLen - 1; i++) {
                minIndex = i;
                for (int j = i + 1; j < dataLen; j++) {
                    if (lstSelection[j] < lstSelection[minIndex]) {     // 寻找最小的数
                        minIndex = j;                 // 将最小数的索引保存
                    }
                }
                temp = lstSelection[i];
                lstSelection[i] = lstSelection[minIndex];
                lstSelection[minIndex] = temp;
                //记录每一次操作
                lstSelectionProcess.Add(Clone<uint>(lstSelection));
                timesSelection += 1;
            }
        }
        public void InsertionSort() {//出现值相同
            List<uint> lstInsertion = new List<uint>();
            lstInsertionProcess.Clear();
            lstInsertion.AddRange(lstOrigin);
            lstInsertionProcess.Add(Clone<uint>(lstInsertion));

            int preIndex; uint current;
            for (int i = 1; i < dataLen; i++) {
                preIndex = i - 1;
                current = lstInsertion[i];
                while (preIndex >= 0 && lstInsertion[preIndex] > current) {
                    lstInsertion[preIndex + 1] = lstInsertion[preIndex];
                    preIndex--;
                    //记录每一次操作
                    lstInsertionProcess.Add(Clone<uint>(lstInsertion));
                    timesInsertion += 1;
                }
                lstInsertion[preIndex + 1] = current;
                //记录每一次操作
                lstInsertionProcess.Add(Clone<uint>(lstInsertion));
                timesInsertion += 1;
            }
        }
        public void ShellSort() {//希尔排序会出现相邻两步相同的情况，但按照算法来讲也是运算了一次的
            List<uint> lstShell = new List<uint>();
            lstShellProcess.Clear();
            lstShell.AddRange(lstOrigin);
            lstShellProcess.Add(Clone<uint>(lstShell));

            uint temp; int gap = 1;
            int j; int i;
            while (gap < dataLen / 3) {          // 动态定义间隔序列
                gap = gap * 3 + 2;
            }
            for (; gap > 0; gap = (int)Math.Floor((decimal)(gap / 3))) {
                for (i = gap; i < dataLen; i++) {
                    temp = lstShell[i];
                    for (j = i - gap; j >= 0 && lstShell[j] > temp; j -= gap) {
                        lstShell[j + gap] = lstShell[j];
                    }
                    lstShell[j + gap] = temp;
                    //记录每一次操作
                    lstShellProcess.Add(Clone<uint>(lstShell));
                    timesShell += 1;
                }
            }
        }
        public void MergeSort() {
            List<uint> lstMerge = new List<uint>();
            lstMergeProcess.Clear();
            lstMerge.AddRange(lstOrigin);
            lstMergeProcess.Add(Clone<uint>(lstMerge));

            MergeSortRecursion(lstMerge, 0, (int)dataLen - 1);          // 递归实现
        }
        public void HeapSort() {
            List<uint> lstHeap = new List<uint>();
            lstHeapProcess.Clear();
            lstHeap.AddRange(lstOrigin);
            lstHeapProcess.Add(Clone<uint>(lstHeap));

            int heap_size = BuildHeap(lstHeap, (int)dataLen);    // 建立一个最大堆
            while (heap_size > 1)           // 堆（无序区）元素个数大于1，未完成排序
            {
                // 将堆顶元素与堆的最后一个元素互换，并从堆中去掉最后一个元素
                // 此处交换操作很有可能把后面元素的稳定性打乱，所以堆排序是不稳定的排序算法
                Swap(lstHeap, 0, --heap_size);
                //记录每一次操作
                lstHeapProcess.Add(Clone<uint>(lstHeap));
                timesHeap += 1;
                Heapify(lstHeap, 0, heap_size);     // 从新的堆顶元素开始向下进行堆调整，时间复杂度O(logn)
            }
        }
        public void QuickSort() {
            List<uint> lstQuick = new List<uint>();
            lstQuickProcess.Clear();
            lstQuick.AddRange(lstOrigin);
            lstQuickProcess.Add(Clone<uint>(lstQuick));

            QuickSortRecursion(lstQuick, 0, (int)dataLen - 1);          // 递归实现
        }

        public static List<T> Clone<T>(object List) {
            using (Stream objectStream = new MemoryStream()) {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, List);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as List<T>;
            }
        }
        private void Merge(List<uint> res, int left, int mid, int right) {
            List<uint> temp = new List<uint>(right - left + 1);
            int i = left, j = mid + 1;

            while (i <= mid && j <= right) {
                temp.Add(res[i] >= res[j] ? res[j++] : res[i++]);
            }
            while (i <= mid) {
                temp.Add(res[i++]);
            }
            while (j <= right) {
                temp.Add(res[j++]);
            }
            for (int k = 0; k < right - left + 1; k++) {
                res[left + k] = temp[k];
                //记录每一次操作
                lstMergeProcess.Add(Clone<uint>(res));
                timesMerge += 1;
            }
        }
        void MergeSortRecursion(List<uint> A, int left, int right)    // 递归实现的归并排序(自顶向下)
{
            if (left == right)    // 当待排序的序列长度为1时，递归开始回溯，进行merge操作
                return;
            int mid = (left + right) / 2;
            MergeSortRecursion(A, left, mid);
            MergeSortRecursion(A, mid + 1, right);
            Merge(A, left, mid, right);
        }
        void Swap(List<uint> A, int i, int j) {
            uint temp = A[i];
            A[i] = A[j];
            A[j] = temp;
        }
        void Heapify(List<uint> A, int i, int size)  // 从A[i]向下进行堆调整
        {
            int left_child = 2 * i + 1;         // 左孩子索引
            int right_child = 2 * i + 2;        // 右孩子索引
            int max = i;                        // 选出当前结点与其左右孩子三者之中的最大值
            if (left_child < size && A[left_child] > A[max])
                max = left_child;
            if (right_child < size && A[right_child] > A[max])
                max = right_child;
            if (max != i) {
                Swap(A, i, max);                // 把当前结点和它的最大(直接)子节点进行交换
                //记录每一次操作
                lstHeapProcess.Add(Clone<uint>(A));
                timesHeap += 1;
                Heapify(A, max, size);          // 递归调用，继续从当前结点向下进行堆调整
            }
        }
        int BuildHeap(List<uint> A, int n)           // 建堆，时间复杂度O(n)
        {
            int heap_size = n;
            for (int i = heap_size / 2 - 1; i >= 0; i--) // 从每一个非叶结点开始向下进行堆调整
                Heapify(A, i, heap_size);
            return heap_size;
        }
        int Partition(List<uint> A, int left, int right) {  // 划分函数
            uint pivot = A[right];               // 这里每次都选择最后一个元素作为基准
            int tail = left - 1;                // tail为小于基准的子数组最后一个元素的索引
            for (int i = left; i < right; i++) {  // 遍历基准以外的其他元素
                if (A[i] <= pivot) {              // 把小于等于基准的元素放到前一个子数组末尾
                    Swap(A, ++tail, i);
                    //记录每一次操作
                    lstQuickProcess.Add(Clone<uint>(A));
                    timesQuick += 1;
                }
            }
            Swap(A, tail + 1, right);           // 最后把基准放到前一个子数组的后边，剩下的子数组既是大于基准的子数组
                                                // 该操作很有可能把后面元素的稳定性打乱，所以快速排序是不稳定的排序算法
            //记录每一次操作
            lstQuickProcess.Add(Clone<uint>(A));
            timesQuick += 1;
            return tail + 1;                    // 返回基准的索引
        }
        public void QuickSortRecursion(List<uint> A, int left, int right) {
            if (left >= right)
                return;
            int pivot_index = Partition(A, left, right); // 基准的索引
            QuickSortRecursion(A, left, pivot_index - 1);
            QuickSortRecursion(A, pivot_index + 1, right);
        }
        /// <summary>
        /// 颜色转换帮助类
        /// </summary>
        public class ColorConversionHelper {
            public static Color GreytoRGB(byte brightness) {
                byte r = 0, g = 0, b = 0;
                r = (byte)(255 - brightness);
                g = (byte)(255 - brightness);
                b = (byte)(255 - brightness);
                return Color.FromRgb(r, g, b);
            }
        }
    }
}
