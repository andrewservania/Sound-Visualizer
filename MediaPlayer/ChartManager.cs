using MediaPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SoundVisualizer
{

    public class ChartManager
    {
        // Variables that ought to stay private
        private Chart chart;
        private Control.ControlCollection control;
        public ChartManager(Chart _chart, Control.ControlCollection _control)
        {
            chart = _chart;
            control = _control;
        }


        private void InitializeChart()
        {
            chart.Series.First().ChartType = SeriesChartType.FastLine;
            chart.Series.First().XValueMember = "X";
            chart.Series.First().YValueMembers = "Y";

            chart.DataSource = GraphAudioBuffer.audioData;
            chart.DataBind();

            //chart.ChartAreas.First().AxisY.Minimum = -32000.0;
            //chart.ChartAreas.First().AxisY.Maximum = 32000.0;

            chart.ChartAreas.First().AxisY.Minimum = -1.0f;
            chart.ChartAreas.First().AxisY.Maximum = 1.0f;

            // Info: https://www.daniweb.com/software-development/csharp/code/451281/simple-line-graph-charting
            control.Add(chart);// is this necessary?
        }


        // Accessor Methods
        public Chart GetChart()
        {
            return chart;
        }

       
    }
}
