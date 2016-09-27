using System.Collections.Generic;
using MonoGame.Squid.Controls;

namespace MonoGame.Squid.Util
{
    public abstract class GuiAction
    {
        internal Control _control;
        internal GuiActionList List;

        protected GuiActionList Actions { get { return List; } }
        protected Control Control { get { return _control; } }

        public event VoidEvent Finished;
        public event VoidEvent Started;
        public event VoidEvent Updated;

        //public int Lanes;
        public bool IsBlocking = true;

        protected bool _isFinished;
        public bool IsFinished
        {
            get { return _isFinished; }
            protected set
            {
                _isFinished = value;

                if (_isFinished)
                {
                    End();

                    if (Finished != null)
                        Finished(_control);
                }
            }
        }

        private bool _firstRun = true;
        internal void InternalUpdate(float dt)
        {
            if (_firstRun)
            {
                _firstRun = false;

                Start();

                if (Started != null)
                    Started(_control);
            }

            Update(dt);

            if (Updated != null)
                Updated(_control);
        }

        public virtual void Start() { }
        public virtual void Update(float dt) { }
        public virtual void End() { }
    }

    public class GuiActionList
    {
        internal bool IsUpdating;

        private int _index;
        private readonly Control _owner;
        private readonly List<GuiAction> _actions = new List<GuiAction>();

        public GuiAction First { get { return _actions.Count > 0 ? _actions[0] : null; } }
        public GuiAction Last { get { return _actions.Count > 0 ? _actions[_actions.Count - 1] : null; } }

        public GuiActionList(Control owner)
        {
            this._owner = owner;
        }

        public void Clear()
        {
            _actions.Clear();
        }

        public GuiAction Add(GuiAction action)
        {
            action.List = this;
            action._control = _owner;
            _actions.Add(action);
            return action;
        }

        public void InsertBefore(GuiAction action)
        {
            action.List = this;
            action._control = _owner;
            _actions.Insert(_index - 1, action);
        }

        public void InsertAfter(GuiAction action)
        {
            action.List = this;
            action._control = _owner;
            _actions.Insert(_index + 1, action);
        }

        public void Remove(GuiAction action)
        {
            _actions.Remove(action);
        }

        public void Update(float dt)
        {
            IsUpdating = true;

            _index = 0;
            var i = _index;
            //int lanes = 0;

            while (i < _actions.Count)
            {
                var action = _actions[i];

                //if ((lanes & action.Lanes) == 0)
                //    continue;

                action.InternalUpdate(dt);

                //if (action.IsBlocking)
                //    lanes |= action.Lanes;

                if (action.IsFinished)
                {
                    // action->OnEnd();
                    _actions.Remove(action);
                }

                i++;
                _index = i;

                if (action.IsBlocking)
                    break;
            }

            IsUpdating = false;
        }
    }
}
