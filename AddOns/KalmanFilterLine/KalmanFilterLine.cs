#region Using declarations
using NinjaTrader.Custom.AddOns.KalmanFilterLine;
using NinjaTrader.Custom.AddOns.KalmanFilterLine.NerdStuff;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class KalmanFilterLine : Indicator
    {
        #region Properties

        public const string GROUP_NAME_GENERAL = "1. General";
        public const string GROUP_NAME_KALMAN_FILTER_LINE = "2. Kalman Filter Line";
        public const string GROUP_NAME_PLOTS = "Plots";

        #region General Properties

        [NinjaScriptProperty, XmlIgnore, ReadOnly(true)]
        [Display(Name = "Version", Description = "Kalman Filter Line", Order = 0, GroupName = GROUP_NAME_GENERAL)]
        public string Version => "1.0.0";

        #endregion

        #region Kalman Filter Line Properties

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Period", Description = "Period used for KFL.", GroupName = GROUP_NAME_KALMAN_FILTER_LINE, Order = 0)]
        public int Period { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Price Type", Description = "Price type used for calculation.", GroupName = GROUP_NAME_KALMAN_FILTER_LINE, Order = 1)]
        public PriceTypeOption PriceType { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Dynamic Kalman Filter Params", Description = "Enable to allow autocorrelation to dynamically update the parameters.", GroupName = GROUP_NAME_KALMAN_FILTER_LINE, Order = 2)]
        public bool EnableDynamicKalmanFilterParams { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Base Q (Process Noise)", Description = "Base Q for Kalman Filter. Increase for quicker model reaction.", GroupName = GROUP_NAME_KALMAN_FILTER_LINE, Order = 3)]
        public double BaseQ { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Base R (Measurement Noise)", Description = "Base R for Kalman Filter. Increase for smoother model reaction.", GroupName = GROUP_NAME_KALMAN_FILTER_LINE, Order = 4)]
        public double BaseR { get; set; }

        #endregion

        #region Plots

        [Browsable(false)]
        [XmlIgnore]
        [Display(Name = "Kalman Filter Line", GroupName = GROUP_NAME_PLOTS, Order = 0)]
        public Series<double> KalmanLine
        {
            get { return Values[0]; }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Kalman Filter Line Opacity", Description = "The opacity for the line. (0 to 255)", GroupName = GROUP_NAME_PLOTS, Order = 1)]
        public byte KalmanLineOpacity { get; set; }

        #endregion

        #endregion

        private ATR _atr;
        private KalmanFilter1D _kf;
        private List<double> _window;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"The smarter moving average... adaptive ATR and autocorrelation-driven moving average.";
                Name = "_KalmanFilterLine";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                Period = 9;
                PriceType = PriceTypeOption.Close;
                EnableDynamicKalmanFilterParams = true;
                BaseQ = 1e-5;
                BaseR = 1e-4;

                AddPlot(Brushes.DodgerBlue, "Kalman Line");
                KalmanLineOpacity = 255;
            }
            else if (State == State.DataLoaded)
            {
                _atr = ATR(Period);
                _kf = new KalmanFilter1D(GetPrice(0), 1, BaseQ, BaseR);
                _window = new List<double>();
            }
            else if (State == State.Configure)
            {
                Plots[0].Opacity = KalmanLineOpacity;
            }
        }

        private double GetPrice(int barsAgo)
        {
            return PriceType switch
            {
                PriceTypeOption.Open => Open[barsAgo],
                PriceTypeOption.High => High[barsAgo],
                PriceTypeOption.Low => Low[barsAgo],
                _ => Close[barsAgo],
            };
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Period)
                return;

            if (IsFirstTickOfBar)
            {
                if (_window.Count >= Period)
                    _window.RemoveAt(0);

                _window.Add(GetPrice(1));
                UpdateKFParams(_kf, _window);
            }

            UpdateKalmanLine();
        }

        private void UpdateKalmanLine()
        {
            // Update plot with filtered value
            Values[0][0] = _kf.Update(GetPrice(0));
        }

        private void UpdateKFParams(KalmanFilter1D filter, List<double> inputSeries)
        {
            if (!EnableDynamicKalmanFilterParams)
                return;

            double absAutocorr = Math.Abs(NerdFunctions.Autocorrelation(inputSeries));
            double qScale = Math.Max(0.1, Math.Min(2.0, 1.0 / (absAutocorr + 0.1)));
            double rScale = Math.Max(0.5, Math.Min(2.0, absAutocorr * 2.0));

            // ATR adjustment for boosting
            double normalizedAtr = _atr[0] / TickSize;
            double atrFactor = 1.0 + (normalizedAtr / 100.0);

            filter.SetNoiseParameters(BaseQ * qScale * atrFactor, BaseR * rScale * atrFactor);
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
    public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
    {
        private KalmanFilterLine[] cacheKalmanFilterLine;
        public KalmanFilterLine KalmanFilterLine(int period, PriceTypeOption priceType, bool enableDynamicKalmanFilterParams, double baseQ, double baseR, byte kalmanLineOpacity)
        {
            return KalmanFilterLine(Input, period, priceType, enableDynamicKalmanFilterParams, baseQ, baseR, kalmanLineOpacity);
        }

        public KalmanFilterLine KalmanFilterLine(ISeries<double> input, int period, PriceTypeOption priceType, bool enableDynamicKalmanFilterParams, double baseQ, double baseR, byte kalmanLineOpacity)
        {
            if (cacheKalmanFilterLine != null)
                for (int idx = 0; idx < cacheKalmanFilterLine.Length; idx++)
                    if (cacheKalmanFilterLine[idx] != null && cacheKalmanFilterLine[idx].Period == period && cacheKalmanFilterLine[idx].PriceType == priceType && cacheKalmanFilterLine[idx].EnableDynamicKalmanFilterParams == enableDynamicKalmanFilterParams && cacheKalmanFilterLine[idx].BaseQ == baseQ && cacheKalmanFilterLine[idx].BaseR == baseR && cacheKalmanFilterLine[idx].KalmanLineOpacity == kalmanLineOpacity && cacheKalmanFilterLine[idx].EqualsInput(input))
                        return cacheKalmanFilterLine[idx];
            return CacheIndicator<KalmanFilterLine>(new KalmanFilterLine() { Period = period, PriceType = priceType, EnableDynamicKalmanFilterParams = enableDynamicKalmanFilterParams, BaseQ = baseQ, BaseR = baseR, KalmanLineOpacity = kalmanLineOpacity }, input, ref cacheKalmanFilterLine);
        }
    }
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
    public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
    {
        public Indicators.KalmanFilterLine KalmanFilterLine(int period, PriceTypeOption priceType, bool enableDynamicKalmanFilterParams, double baseQ, double baseR, byte kalmanLineOpacity)
        {
            return indicator.KalmanFilterLine(Input, period, priceType, enableDynamicKalmanFilterParams, baseQ, baseR, kalmanLineOpacity);
        }

        public Indicators.KalmanFilterLine KalmanFilterLine(ISeries<double> input, int period, PriceTypeOption priceType, bool enableDynamicKalmanFilterParams, double baseQ, double baseR, byte kalmanLineOpacity)
        {
            return indicator.KalmanFilterLine(input, period, priceType, enableDynamicKalmanFilterParams, baseQ, baseR, kalmanLineOpacity);
        }
    }
}

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
    {
        public Indicators.KalmanFilterLine KalmanFilterLine(int period, PriceTypeOption priceType, bool enableDynamicKalmanFilterParams, double baseQ, double baseR, byte kalmanLineOpacity)
        {
            return indicator.KalmanFilterLine(Input, period, priceType, enableDynamicKalmanFilterParams, baseQ, baseR, kalmanLineOpacity);
        }

        public Indicators.KalmanFilterLine KalmanFilterLine(ISeries<double> input, int period, PriceTypeOption priceType, bool enableDynamicKalmanFilterParams, double baseQ, double baseR, byte kalmanLineOpacity)
        {
            return indicator.KalmanFilterLine(input, period, priceType, enableDynamicKalmanFilterParams, baseQ, baseR, kalmanLineOpacity);
        }
    }
}

#endregion
