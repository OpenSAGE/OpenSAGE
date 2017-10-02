using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;
using OpenSage.Data;
using OpenSage.DataViewer.Framework;

namespace OpenSage.DataViewer.ViewModels
{
    public abstract class FileContentViewModelBase : PropertyChangedBase, IDisposable
    {
        public Game Game { get; }

        protected FileContentViewModelBase()
        {
            Game = IoC.Get<GameService>().Game;
        }

        public void Dispose()
        {
            Game.SetSwapChain(null);
            Game.Input.InputProvider = null;

            Game.Scene = null;

            Game.ContentManager.Unload();
        }
    }

    public abstract class FileContentViewModel : FileContentViewModelBase
    {
        public static FileContentViewModel Create(FileSystemEntry file)
        {
            switch (Path.GetExtension(file.FilePath).ToLower())
            {
                case ".w3d":
                    return new W3dFileContentViewModel(file);

                case ".dds":
                case ".tga":
                    return new TextureFileContentViewModel(file);

                case ".map":
                    return new MapFileContentViewModel(file);

                case ".wav":
                case ".mp3":
                case ".bik":
                    return new AudioVideoFileContentViewModel(file);

                case ".ani":
                    return new AnimatedCursorFileContentViewModel(file);

                case ".bmp":
                    return new BitmapFileContentViewModel(file);

                case ".csf":
                    return new CsfFileContentViewModel(file);

                case ".wnd":
                    return new WndFileContentViewModel(file);

                case ".ini":
                    return new IniFileContentViewModel(file);

                case ".txt":
                    return new TextFileContentViewModel(file);

                default:
                    return new UnsupportedFileContentViewModel(file);
            }
        }

        public FileSystemEntry File { get; }

        protected FileContentViewModel(FileSystemEntry file)
        {
            File = file;
        }
    }

    public abstract class FileContentViewModel<TSubObject> : FileContentViewModel
        where TSubObject : FileSubObjectViewModel
    {
        private IReadOnlyList<TSubObject> _subObjects;
        public IReadOnlyList<TSubObject> SubObjects
        {
            get
            {
                if (_subObjects == null)
                {
                    _subObjects = CreateSubObjects();
                    if (_subObjects.Count > 0)
                    {
                        SelectedSubObject = _subObjects[0];
                    }
                }
                return _subObjects;
            }
        }

        protected abstract IReadOnlyList<TSubObject> CreateSubObjects();

        private TSubObject _selectedSubObject;

        public TSubObject SelectedSubObject
        {
            get { return _selectedSubObject; }
            set
            {
                _selectedSubObject?.Deactivate();

                _selectedSubObject = value;

                _selectedSubObject?.Activate();

                OnSelectedSubObjectChanged(_selectedSubObject);

                NotifyOfPropertyChange();
            }
        }

        protected virtual void OnSelectedSubObjectChanged(TSubObject subObject) { }

        protected FileContentViewModel(FileSystemEntry file) 
            : base(file)
        {
        }
    }

    public abstract class FileSubObjectViewModel : FileContentViewModelBase
    {
        public abstract string GroupName { get; }
        public abstract string Name { get; }

        public virtual void Activate() { }

        public virtual void Deactivate() { }
    }
}
