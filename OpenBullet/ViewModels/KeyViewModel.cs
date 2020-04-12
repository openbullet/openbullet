﻿using OpenBullet.Views.StackerBlocks;
using RuriLib;
using RuriLib.Functions.Conditions;
using RuriLib.Models;
using RuriLib.ViewModels;

namespace OpenBullet.ViewModels
{
    public class KeyViewModel : ViewModelBase
    {
        private KeyFullId id;
        public KeyFullId Id { get { return id; } set { id = value; OnPropertyChanged(); } }

        public Key Key { get; set; }
        public string LeftTerm { get { return Key.LeftTerm; } set { Key.LeftTerm = value; OnPropertyChanged(); } }
        public Comparer Comparer { get { return Key.Comparer; } set { Key.Comparer = value; OnPropertyChanged(); } }
        public string RightTerm { get { return Key.RightTerm; } set { Key.RightTerm = value; OnPropertyChanged(); } }

        public KeyViewModel(Key key, int id, int parentId)
        {
            Key = key;
            Id = new KeyFullId() { KeyId = id, ParentId = parentId };
            OnPropertyChanged("FullId");
            OnPropertyChanged("Id");
        }
    }
}
