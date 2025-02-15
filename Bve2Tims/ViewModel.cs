using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bve2Tims
{
    internal class ViewModel
    {
        #region Fields

        private Model model;


        #endregion

        #region Static Properties

        public string Unit0
        {
            get { return Model.UnitIndexes.ElementAtOrDefault(0).ToString(); }
            set { Model.UnitIndexes[0] = int.Parse(value); }
        }

        public string Unit1
        {
            get { return Model.UnitIndexes.ElementAtOrDefault(1).ToString(); }
            set { Model.UnitIndexes[1] = int.Parse(value); }
        }

        public string Unit2
        {
            get { return Model.UnitIndexes.ElementAtOrDefault(2).ToString(); }
            set { Model.UnitIndexes[2] = int.Parse(value); }
        }

        public string Unit3
        {
            get { return Model.UnitIndexes.ElementAtOrDefault(3).ToString(); }
            set { Model.UnitIndexes[3] = int.Parse(value); }
        }
        public string Unit4
        {
            get { return Model.UnitIndexes.ElementAtOrDefault(4).ToString(); }
            set { Model.UnitIndexes[4] = int.Parse(value); }
        }

        public string Door0
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(0).ToString(); }
            set { Model.DoorIndexes[0] = int.Parse(value); }
        }

        public string Door1
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(1).ToString(); }
            set { Model.DoorIndexes[1] = int.Parse(value); }
        }

        public string Door2
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(2).ToString(); }
            set { Model.DoorIndexes[2] = int.Parse(value); }
        }

        public string Door3
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(3).ToString(); }
            set { Model.DoorIndexes[3] = int.Parse(value); }
        }

        public string Door4
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(4).ToString(); }
            set { Model.DoorIndexes[4] = int.Parse(value); }
        }

        public string Door5
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(5).ToString(); }
            set { Model.DoorIndexes[5] = int.Parse(value); }
        }

        public string Door6
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(6).ToString(); }
            set { Model.DoorIndexes[6] = int.Parse(value); }
        }

        public string Door7
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(7).ToString(); }
            set { Model.DoorIndexes[7] = int.Parse(value); }
        }

        public string Door8
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(8).ToString(); }
            set { Model.DoorIndexes[8] = int.Parse(value); }
        }

        public string Door9
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(9).ToString(); }
            set { Model.DoorIndexes[9] = int.Parse(value); }
        }

        public string Door10
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(10).ToString(); }
            set { Model.DoorIndexes[10] = int.Parse(value); }
        }

        public string Door11
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(11).ToString(); }
            set { Model.DoorIndexes[11] = int.Parse(value); }
        }

        public string Door12
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(12).ToString(); }
            set { Model.DoorIndexes[12] = int.Parse(value); }
        }

        public string Door13
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(13).ToString(); }
            set { Model.DoorIndexes[13] = int.Parse(value); }
        }

        public string Door14
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(14).ToString(); }
            set { Model.DoorIndexes[14] = int.Parse(value); }
        }

        public string Door15
        {
            get { return Model.DoorIndexes.ElementAtOrDefault(15).ToString(); }
            set { Model.DoorIndexes[15] = int.Parse(value); }
        }

        #endregion

        #region Properties

        /// <summary>
        /// ユニットインデックス
        /// </summary>
        public bool AutoStart
        {
            get { return model.AutoStart; }
            set { model.AutoStart = value; }
        }

        /// <summary>
        /// 通信状態
        /// </summary>
        public bool Status
        {
            get { return model.Status; }
        }

        /// <summary>
        /// 選択中の宛先インデックス
        /// </summary>
        public int SelectedDestinationIndex
        {
            get { return model.SelectedDestinationIndex; }
            set { model.SelectedDestinationIndex = value; }
        }

        /// <summary>
        /// 選択中の宛先
        /// </summary>
        public Udp SelectedDestination
        {
            get { return model.SelectedDestination; }
            set { model.SelectedDestination = value; }
        }

        /// <summary>
        /// 宛先リスト
        /// </summary>
        public ObservableCollection<Udp> Destinations
        {
            get { return new ObservableCollection<Udp>(model.Destinations); }
            set { model.Destinations = new List<Udp>(value); }
        }

        #endregion

        public ViewModel()
        {
            model = new Model();
        }

    }
}
