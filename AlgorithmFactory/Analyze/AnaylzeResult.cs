using System;
using System.Collections.Generic;

namespace QuantConnect.AlgorithmFactory.Analyze
{
    /// <summary>
    /// Base class for Algorithm Analysis
    /// </summary>
    public class AnalysisResultBase
    {
        public AnalysisResult Result { get; set; }
        /// <summary>
        /// Base constructor for algorithm analysis
        /// </summary>
        /// <param name="result"></param>
        public AnalysisResultBase(AnalysisResult result)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Class representing an unsuccessful analysis
    /// </summary>
    public class UnsuccessfulAnalysis : AnalysisResultBase
    {
        /// <summary>
        /// Constructor for <see cref="UnsuccessfulAnalysis"/>
        /// </summary>
        /// <param name="errorMessage">A single error message</param>
        public UnsuccessfulAnalysis(string errorMessage) : base(AnalysisResult.Failure)
        {
            FailureMessage.Add(errorMessage);
        }

        /// <summary>
        /// Constructor of <see cref="UnsuccessfulAnalysis"/>
        /// </summary>
        /// <param name="errorMessages">A list of error messages</param>
        public UnsuccessfulAnalysis(List<string> errorMessages) : base(AnalysisResult.Failure)
        {
            FailureMessage.AddRange(errorMessages);
        }

        /// <summary>
        /// Error messages received when analyzing the algorithm
        /// </summary>
        public List<string> FailureMessage = new List<string>();
    }

    /// <summary>
    /// Class that is returned when algorithm is successfully analyzed
    /// </summary>
    public class SuccessfulAnalysis : AnalysisResultBase
    {
        /// <summary>
        /// Constructor for <see cref="SuccessfulAnalysis"/>
        /// </summary>
        /// <param name="start">Start date of the algorithm</param>
        /// <param name="end">End date of the algorithm</param>
        /// <param name="capital">Capital of the algorithm</param>
        /// <param name="tradeableDates">Tradeable dates of the algorithm</param>
        public SuccessfulAnalysis(DateTime start, DateTime end, decimal capital, int tradeableDates)
            : base(AnalysisResult.Success)
        {
            PeriodStart = start;
            PeriodEnd = end;
            TradeableDates = tradeableDates;
            StartingCapital = capital;
        }

        /// <summary>
        /// Start date of the algorithm
        /// </summary>
        public DateTime PeriodStart { get; private set; }
        /// <summary>
        /// End date of the algorithm
        /// </summary>
        public DateTime PeriodEnd { get; private set; }
        /// <summary>
        /// Starting capital of the algorithm
        /// </summary>
        public decimal StartingCapital { get; private set; }
        /// <summary>
        /// Tradable dates of the algorithm
        /// </summary>
        public int TradeableDates { get; private set; }
    }

    /// <summary>
    /// Enum representing the possible analysis results
    /// </summary>
    public enum AnalysisResult
    {
        /// <summary>
        /// Successfully analyzed algorithm
        /// </summary>
        Success,
        /// <summary>
        /// Unsuccessfully analyzed algorithm
        /// </summary>
        Failure
    }
}
