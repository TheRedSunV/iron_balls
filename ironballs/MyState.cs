using ImGuiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Urho3DNet;

namespace ironballs
{
    public partial class MyState : ApplicationState
    {
        protected readonly Scene _scene;
        private readonly UrhoPluginApplication _app;
        private readonly Node _cameraNode;
        private readonly Node _arrowNode;
        private readonly Node _selectNode;
        private readonly Camera _camera;
        private readonly Viewport _viewport;
        private readonly InputMap _inputMap;
        private Random _random = new Random();
        private Node _selectedBall;
        private Node _lastBall;
        private Node _trajectory = null;
        private CustomGeometry _customGeometry = null;

        private Vector3 _curVelocity;
        private float _inertia = 0.8f;
        private float _speed = 0.02f;
        private int _hitTimer = 0;

        private List<Node> team1 = new List<Node>();
        private List<Node> team2 = new List<Node>();
        private List<Node> team3 = new List<Node>();
        private List<Node> team4 = new List<Node>();
        private string activeTeam = "team1";
        private bool t1IsPlayer = false;
        private bool t2IsPlayer = false;
        private bool t3IsPlayer = false;
        private bool t4IsPlayer = false;
        private bool nowPlayer = true;



        public MyState(UrhoPluginApplication app) : base(app.Context)
        {
            MouseMode = MouseMode.MmFree;
            IsMouseVisible = true;

            _app = app;
            _scene = Context.CreateObject<Scene>();
            _scene.Load("Scenes/Scene.scene");
            _cameraNode = _scene.FindChild("CameraNode", true);
            _arrowNode = _scene.FindChild("ArrowNode", true);
            _selectNode = _scene.FindChild("SelectNode", true);
            _selectedBall = _scene.FindChild("ball1", true);
            _lastBall = _selectedBall;
            var temp = _scene.GetChildren();
            foreach (var child in temp)
            {
                if (child.Name == "ball1") team1.Add(child);
                if (child.Name == "ball2") team2.Add(child);
                if (child.Name == "ball3") team3.Add(child);
                if (child.Name == "ball4") team4.Add(child);
            }
            _camera = _cameraNode.GetComponent<Camera>();
            _viewport = Context.CreateObject<Viewport>();
            _viewport.Scene = _scene;
            _viewport.Camera = _camera;
            SetViewport(0, _viewport);
            _inputMap = Context.ResourceCache.GetResource<InputMap>("Input/MoveAndOrbit.inputmap");
            SetupPlayers();
            if (!t1IsPlayer) nowPlayer = false;

            Material transparentMaterial = new Material(Context);
            transparentMaterial.CullMode = CullMode.CullNone;
            transparentMaterial.NumTechniques = 1;
            transparentMaterial.SetTechnique(0, GetSubsystem<ResourceCache>().GetResource<Technique>("Techniques/NoTextureUnlitAlpha.xml"));
            transparentMaterial.SetShaderParameter("MatDiffColor", Color.White);
            transparentMaterial.VertexShaderDefines = "VERTEXCOLOR";
            transparentMaterial.PixelShaderDefines = "VERTEXCOLOR";



            _trajectory = _scene.CreateChild("Traectory");
            _customGeometry = _trajectory.CreateComponent<CustomGeometry>();
            _customGeometry.SetMaterial(transparentMaterial);
        }

        public void SetupPlayers()
        {
            if (MySetup.Players == 0)
            {
                t1IsPlayer = false;
                t2IsPlayer = false;
                t3IsPlayer = false;
                t4IsPlayer = false;
            }
            if (MySetup.Players == 1)
            {
                t1IsPlayer = true;
                t2IsPlayer = false;
                t3IsPlayer = false;
                t4IsPlayer = false;
            }
            if (MySetup.Players == 2)
            {
                t1IsPlayer = true;
                t2IsPlayer = true;
                t3IsPlayer = false;
                t4IsPlayer = false;
            }
            if (MySetup.Players == 3)
            {
                t1IsPlayer = true;
                t2IsPlayer = true;
                t3IsPlayer = true;
                t4IsPlayer = false;
            }
            if (MySetup.Players == 4)
            {
                t1IsPlayer = true;
                t2IsPlayer = true;
                t3IsPlayer = true;
                t4IsPlayer = true;
            }
        }

        public override void Update(float timeStep)
        {
            base.Update(timeStep);
            if (_selectedBall!=null)
            {
                _selectNode.Position = new Vector3(_selectedBall.Position.X, 0.6f, _selectedBall.Position.Z);
            }

            if (_hitTimer > 0) 
            {
                HideTrajectory();
                _hitTimer--;
                if (_hitTimer == 2)
                {
                    RemoveBalls();
                    NextPlayer();
                }
            } 
            else if (nowPlayer)
            {
                var left = _inputMap.Evaluate("Left");
                var right = _inputMap.Evaluate("Right");
                var forward = _inputMap.Evaluate("Forward");
                var back = _inputMap.Evaluate("Back");

                var velocity = new Vector3(forward - back, 0, left - right);
                velocity.Normalize();
                _curVelocity = _inertia * _curVelocity + (1 - _inertia) * velocity * _speed;
                var newPosition = _arrowNode.Position + _curVelocity;
                //newPosition.X = MyTools.Clamp(newPosition.X, -6.1f, 6.1f);
                //newPosition.Z = MyTools.Clamp(newPosition.Z, -1.5f, 6.6f);
                _arrowNode.Position = newPosition;
                //_arrowNode.Direction = (_arrowNode.Position - _selectedBall.Position).Normalized;
                ArrowDir();
                ShowTrajectory();

                if (_inputMap.Evaluate("Use") > 0.5f && _hitTimer < 1) Hit();

            }
            else
            {
                Hit();
            }


            //debug
            ImGui.Begin("Debug");
            ImGui.Text(activeTeam);
            ImGui.Text(_hitTimer.ToString());
            ImGui.Text("Team 1 count:" + team1.Count.ToString());
            ImGui.Text("Team 2 count:" + team2.Count.ToString());
            ImGui.Text("Team 3 count:" + team3.Count.ToString());
            ImGui.Text("Team 4 count:" + team4.Count.ToString());
            ImGui.Text(EndPosition().ToString());
            ImGui.Text(_selectedBall.Position.ToString());
            ImGui.Text(_arrowNode.Position.ToString());
            ImGui.End();
        }

        public void NextPlayer()
        {
            //t1
            if (activeTeam == "team1")
            {
                activeTeam = "team2";
                if (team2.Count > 0)
                {
                    _random = new Random();
                    var r = _random.Next(0,team2.Count-1);
                    _selectedBall = team2[r];
                    if (t2IsPlayer)
                    {
                        nowPlayer = true;
                        _arrowNode.Position = new Vector3(0, 0.25f, 0);
                    }
                    else
                    {
                        nowPlayer = false;
                        AiAroowPos();
                    }
                }
                else NextPlayer();
            }
            //t2
            else if (activeTeam == "team2")
            {
                activeTeam = "team3";
                if (team3.Count > 0)
                {
                    _random = new Random();
                    var r = _random.Next(0, team3.Count - 1);
                    _selectedBall = team3[r];
                    if (t3IsPlayer)
                    {
                        nowPlayer = true;
                        _arrowNode.Position = new Vector3(0, 0.25f, 0);
                    }
                    else
                    {
                        nowPlayer = false;
                        AiRandomPos();
                    }
                }
                else NextPlayer();
            }
            //t3
            else if (activeTeam == "team3")
            {
                activeTeam = "team4";
                if (team4.Count > 0)
                {
                    _random = new Random();
                    var r = _random.Next(0, team4.Count - 1);
                    _selectedBall = team4[r];
                    if (t4IsPlayer)
                    {
                        nowPlayer = true;
                        _arrowNode.Position = new Vector3(0, 0.25f, 0);
                    }
                    else
                    {
                        nowPlayer = false;
                        AiAroowPos();
                    }
                }
                else NextPlayer();
            }
            //t4
            else if (activeTeam == "team4")
            {
                activeTeam = "team1";
                if (team1.Count > 0)
                {
                    var r = _random.Next(0, team1.Count - 1);
                    _selectedBall = team1[r];
                    if (t1IsPlayer)
                    {
                        nowPlayer = true;
                        _arrowNode.Position = new Vector3(0, 0.25f, 0);
                    }
                    else
                    {
                        nowPlayer = false;
                        AiNullPos();
                    }
                }
                else NextPlayer();
            }
        }

        public void AiAroowPos()
        {
            var v = new Vector3(_selectedBall.Position.X * 2, 0.25f, _selectedBall.Position.Z * 2);
            _arrowNode.Position = v;
            ArrowDir();
        }
        public void AiRandomPos()
        {
            var v = new Vector3(_random.Next(-4,4), 0.25f, _random.Next(-4, 4));
            _arrowNode.Position = v;
            ArrowDir();
        }
        public void AiLastPos()
        {
            if (_lastBall != null)
            {
                var t = _lastBall.Position - _selectedBall.Position;
                var v = new Vector3(t.X * 2, 0.25f, t.Z * 2);
                _arrowNode.Position = v;
                ArrowDir();
            }
            else AiRandomPos();
        }
        public void AiNullPos()
        {
            _arrowNode.Position = new Vector3(0, 0.25f, 0);
            ArrowDir();
        }

        public void Hit()
        {
            var body = _selectedBall.GetComponent<RigidBody>();
            body.ApplyImpulse(( _selectedBall.Position - _arrowNode.Position).Normalized * 1.15f);
            _lastBall = _selectedBall;
            _hitTimer = 300;
        }

        public void ShowTrajectory()
        {
            _trajectory.IsEnabled = true;
            var e = EndPosition();
            _customGeometry.BeginGeometry(0, PrimitiveType.LineStrip);
            GeometryBuilder builder = new GeometryBuilder(_customGeometry);
            SimpleVertex[] frame = new SimpleVertex[4];
            frame[0] = new SimpleVertex(new Vector3(_selectedBall.Position.X, 0.35f, _selectedBall.Position.Z), Color.Red);
            frame[1] = new SimpleVertex(new Vector3(e.X, 0.35f, e.Z), Color.Red);
            frame[2] = new SimpleVertex(new Vector3(e.X, 0.3f, e.Z), Color.Red);
            frame[3] = new SimpleVertex(new Vector3(_selectedBall.Position.X, 0.3f, _selectedBall.Position.Z), Color.Red);
            builder.BuildSolidQuad(frame);

            _customGeometry.Commit();
        }

        public void HideTrajectory()
        {
            _trajectory.IsEnabled = false;
        }

        public Vector3 EndPosition()
        {
            try
            {
                var _raycastResult = new PhysicsRaycastResult();
                var world = _scene.GetComponent<PhysicsWorld>();
                world.RaycastSingle(_raycastResult, new Ray(_selectedBall.WorldPosition, - _arrowNode.WorldDirection),
                    10f);
                var selectedNode = _raycastResult.Body?.Node;
                return _raycastResult.Position;

            }
            catch 
            {
                return Vector3.Zero;
            }
        }

        public void RemoveBalls()
        {
            foreach (var ball in team1.ToList())
            {
                if (ball.Position.Y < -2)
                {
                    team1.Remove(ball);
                    ball.Remove();
                }
            }
            foreach (var ball in team2.ToList())
            {
                if (ball.Position.Y < -2)
                {
                    team2.Remove(ball);
                    ball.Remove();
                }
            }
            foreach (var ball in team3.ToList())
            {
                if (ball.Position.Y < -2)
                {
                    team3.Remove(ball);
                    ball.Remove();
                }
            }
            foreach (var ball in team4.ToList())
            {
                if (ball.Position.Y < -2)
                {
                    team4.Remove(ball);
                    ball.Remove();
                }
            }
        }

        public void ArrowDir()
        {
            _arrowNode.Direction = new Vector3 (_arrowNode.Position.X - _selectedBall.Position.X, 0, _arrowNode.Position.Z - _selectedBall.Position.Z).Normalized;
        }


        public override void Activate(StringVariantMap bundle)
        {
            SubscribeToEvent(E.KeyUp, HandleKeyUp);

            _scene.IsUpdateEnabled = true;

            base.Activate(bundle);
        }

        public override void Deactivate()
        {
            _scene.IsUpdateEnabled = false;
            UnsubscribeFromEvent(E.KeyUp);
            base.Deactivate();
        }

        protected override void Dispose(bool disposing)
        {
            _scene?.Dispose();

            base.Dispose(disposing);
        }

        private void HandleKeyUp(VariantMap args)
        {
            var key = (Key)args[E.KeyUp.Key].Int;
            switch (key)
            {
                case Key.KeyEscape:
                case Key.KeyBackspace:
                    _app.HandleBackKey();
                    return;
            }
        }
    }
}
