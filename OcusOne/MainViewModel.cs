using System.Collections.Generic;

using OxyPlot;

namespace OcusOne
{
    public class MainViewModel
    {

        public MainViewModel()
        {

            this.Title = "Example 2";
            this.Points = new List<DataPoint>
            {
                 new DataPoint(0,4),

                 new DataPoint(0,4),

                 new DataPoint(0,4),

                 new DataPoint(0,4),

                 new DataPoint(0,4),

                 new DataPoint(0,4)

            };
                 
           
                                    
        }

        public string Title { get; private set; }

        public IList<DataPoint> Points { get; private set; }
    }
}