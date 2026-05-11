using System;
using System.Collections.Generic;
using UnityEngine.XR;

    public class StateMachine
    {
        private StateNode _current;
        private Dictionary<Type, StateNode> _nodes = new Dictionary<Type, StateNode>();
        HashSet<Transition> anyTransitions = new HashSet<Transition>();

        public void Update()
        {
            var transition = GetTransition();
            if (transition != null)
            {
                ChangeState(transition.To);
            }
            _current.State.OnUpdate();
        }
        public IState CurrentState => _current.State;

        public void FixedUpdate()
        {
            _current.State.OnFixedUpdate();
        }

        public void SetState(IState state)
        {
            _current=_nodes[state.GetType()];
            _current.State.OnEnter();
        }

        void ChangeState(IState state)
        {
            if (state == _current.State)
                return;
            var previousState=_current.State;
            var nextState = _nodes[state.GetType()].State;
            previousState.OnExit();
            nextState.OnEnter();
            _current=_nodes[state.GetType()];
        }

        Transition GetTransition()
        {
            foreach (var transition in anyTransitions)
            {
                if(transition.Evaluate())
                    return transition;
            }
            foreach (var transition in _current.Transitions)
            {
                if(transition.Evaluate())
                    return transition;
            }
            return null;
        }
        public void AddTransition<T>(IState from, IState to, T conditions)
        {
            GetorAddNode(from).AddTransition(GetorAddNode(to).State, conditions);
        }
        public void AddAnyTransition<T>(IState to, T conditions)
        {
            anyTransitions.Add(new Transition<T>(GetorAddNode(to).State, conditions));
        }

        StateNode GetorAddNode(IState state)
        {
            var node=_nodes.GetValueOrDefault(state.GetType());
            if (node == null)
            {
                node = new StateNode(state);
                _nodes.Add(state.GetType(), node);
            }
            return node;
        }

        class StateNode
        {
            public IState State { get; }
            public HashSet<Transition> Transitions { get; }

            public StateNode(IState state)
            {
                this.State = state;
                this.Transitions = new HashSet<Transition>();
            }

            public void AddTransition<T>(IState To,T Conditions)
            {
                Transitions.Add(new Transition<T>(To, Conditions));
                
            }
            
        }
        
    }