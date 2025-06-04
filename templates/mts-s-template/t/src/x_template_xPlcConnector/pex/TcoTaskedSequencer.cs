using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TcoCore;
using TcOpen.Inxton.RepositoryDataSet;
using Vortex.Connector;

namespace x_template_xPlc
{
    public partial class TcoTaskedSequencer : ITcoTasked
    {
        public string _setId { get; private set; }
        public RepositoryDataSetHandler<SequencerData> _dataHandler { get; private set; }
        public EntitySet<SequencerData> CurrentDataSet { get; private set; } = new EntitySet<SequencerData>();

        public void WithDataExport(RepositoryDataSetHandler<SequencerData> dataHandler)
        {
 
          
            _dataHandler = dataHandler;
            _customObserver._exportSeqData.Initialize(ExportData);
           
        }


        private void ExportData()
        {
            var plainData = new PlainTcoCustomSequencerObserver();
            this._customObserver.Read();
            this._customObserver.FlushOnlineToPlain(plainData);
            var startTime = new DateTime(plainData._dataToExport.StartTime.Year, plainData._dataToExport.StartTime.Month, plainData._dataToExport.StartTime.Day, plainData._dataToExport.StartTime.Hour, plainData._dataToExport.StartTime.Minute, plainData._dataToExport.StartTime.Second, plainData._dataToExport.StartTime.Milliseconds);
            var endTime = new DateTime(plainData._dataToExport.EndTime.Year, plainData._dataToExport.EndTime.Month, plainData._dataToExport.EndTime.Day, plainData._dataToExport.EndTime.Hour, plainData._dataToExport.EndTime.Minute, plainData._dataToExport.EndTime.Second, plainData._dataToExport.EndTime.Milliseconds);
            var seqData = new SequencerData() { Symbol = this.Symbol, ControlledUintId=plainData._dataToExport.CuId, CycleId = plainData._dataToExport.CycleId, StartTime = startTime, EndTime = endTime };

           seqData.Steps.Clear();
            foreach (var item in plainData._dataToExport.Steps)
            {
                if (item.TimeStamp.Year !=0)
                {
                    seqData.Steps.Add(new StepExecutions() { Description = item.Description, Duration = item.Duration, ID = item.ID, TimeStamp = new DateTime(item.TimeStamp.Year, item.TimeStamp.Month, item.TimeStamp.Day, item.TimeStamp.Hour, item.TimeStamp.Minute, item.TimeStamp.Second, item.TimeStamp.Milliseconds) });

                }
            }
            
            SaveDataSet(this.Symbol+ DateTime.Now.ToString("_yyyyMMdd_HHmmssfff") ,new EntitySet<SequencerData>() { Item = seqData } );
        }
        public void LoadDataSet(string setId)
        {

            var result = _dataHandler.Repository.Queryable.FirstOrDefault(p => p._EntityId == setId);

            if (result == null)
            {
                
                _dataHandler.Create(setId, CurrentDataSet);

            }
            else
                CurrentDataSet = _dataHandler.Read(setId);


        }
        public void SaveDataSet(string setId ,EntitySet<SequencerData> setData)
        {
         
            EntitySet<SequencerData> result;
            try
            {
                result = _dataHandler.Repository.Queryable.FirstOrDefault(p => p._EntityId == setId);

            }
            catch (Exception)
            {

                throw;
            }

                setData._Modified= DateTime.Now;
                setData._Created = DateTime.Now;
                _dataHandler.Create(setId,setData);
           
           

        }
    }
    /// <summary>
    /// Represents the data collected for a single production cycle executed by a sequencer.
    /// </summary>
    public class SequencerData
    {
        private List<StepExecutions> steps = new List<StepExecutions>();

        /// <summary>
        /// List of all step executions that occurred during this cycle, including repeats.
        /// </summary>
        public List<StepExecutions> Steps
        {
            get => steps;
            set => steps = value;
        }

        /// <summary>
        /// Unique identifier for this production cycle (can include produced entity, timestamp or sequence index ).
        /// </summary>
        public string CycleId { get; set; }

        /// <summary>
        /// Logical identifier of the controlled unit (e.g. station, machine).
        /// </summary>
        public ushort ControlledUintId { get; set; }

        /// <summary>
        /// Symbol name representing the sequencer source (e.g. PLC symbol path).
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Timestamp marking the start of the cycle.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Timestamp marking the end of the cycle.
        /// </summary>
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// Represents a single execution of a step in a sequencer
    /// </summary>
    public class StepExecutions
    {
        /// <summary>
        /// The unique identifier of the step (e.g., as defined in PLC ).
        /// </summary>
        public short ID { get; set; }

        /// <summary>
        /// A human-readable description of the step (e.g., "Open Entity Data").
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The duration of this specific execution of the step.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The timestamp when this execution of the step ended (or was logged).
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }

}

