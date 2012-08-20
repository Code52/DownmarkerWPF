using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public abstract class SiteItemBase : PropertyChangedBase, ISiteItem
    {
        protected readonly IEventAggregator EventAggregator;
        bool isRenaming;
        bool selected;

        protected SiteItemBase(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            EventAggregator.Subscribe(this);
            Children = new ObservableCollection<ISiteItem>();
        }

        public string Name { get; set; }

        public ObservableCollection<ISiteItem> Children { get; protected set; }

        public abstract void CommitRename();
        public abstract void UndoRename();
        public abstract void Delete();

        public bool Selected
        {
            get { return selected; }
            set
            {
                // if we are selected and renaming, and now being deselected
                if (selected && IsRenaming && !value)
                    CommitRename();
                selected = value;
                NotifyOfPropertyChange(() => Selected);
            }
        }

        public bool IsRenaming
        {
            get { return isRenaming; }
            set
            {
                if (!Selected)
                    throw new InvalidOperationException("Item must be selected to rename");
                isRenaming = value;
                NotifyOfPropertyChange(() => IsRenaming);
            }
        }

        public virtual void Dispose()
        {
            EventAggregator.Unsubscribe(this);
            foreach (var child in Children)
            {
                child.Dispose();
            }
        }
    }
}