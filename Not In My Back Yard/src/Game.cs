﻿using FontStash.NET;
using FontStash.NET.GL.Legacy;
using NIMBY.Audio;
using NIMBY.States;
using NIMBY.Utils;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using System;

namespace NIMBY
{
    public unsafe class Game : IDisposable
    {

        private readonly string _title;
        private WindowHandle* _window;
        private uint _width, _height;

        private readonly StateManager _stateManager;

        private GL _gl;
        private Silk.NET.OpenGL.Legacy.GL _legacyGl;
        private Glfw _glfw;

        private Camera _camera;

        private GLFons _glFons;
        private Fontstash _fons;

        public Fontstash Fons => _fons;

        public uint Witdh => _width;

        public uint Height => _height;

        public GL Gl => _gl;

        public Silk.NET.OpenGL.Legacy.GL LegacyGl => _legacyGl;

        public Glfw Glfw => _glfw;

        public Camera Camera => _camera;

        public Game(uint width, uint height, string title)
        {
            _width = width;
            _height = height;
            _title = title;

            _stateManager = new StateManager(this);
            _glfw = Glfw.GetApi();
        }

        private void Init()
        {
            var context = new GlfwContext(_glfw, _window);
            _gl = GL.GetApi(context);
            _legacyGl = Silk.NET.OpenGL.Legacy.GL.GetApi(context);

            ResourceManager.Init(_gl);
            Input.Init(_glfw, _window);

            _glFons = new GLFons(_legacyGl);
            _fons = _glFons.Create(512, 512, (int)FonsFlags.ZeroTopleft);
            _fons.AddFont("stdfont", "./Assets/Fonts/DroidSerif-Regular.ttf");

            _camera = new Camera(0.0f, 0.0f, 0.0f, 0.0f);

            _stateManager.AddState("Main Menu", new MenuState());
            _stateManager.AddState("Level Selector", new LevelSelectorState());
            _stateManager.AddState("Game", new GameState());

            MusicMaster.Start();
        }

        private void Update(double d)
        {
            _camera.Update((float)d);
            _stateManager.Update((float)d);

            Input.Update();
        }

        private void Render()
        {
            _gl.Viewport(0, 0, _width, _height);
            _gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            _gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            _stateManager.Render();
        }

        private void Close()
        {
            ResourceManager.Clear();
            _stateManager.Dispose();
            _glFons.Dispose();
        }

        public void Exit()
        {
            _glfw.SetWindowShouldClose(_window, true);
        }

        public void Dispose()
        {
            MusicMaster.Running = false;
            AudioManager.Stop();
            AudioManager.Finish();
        }

        public void Run()
        {
            _glfw.Init();
            _glfw.WindowHint(WindowHintBool.Resizable, false);
            _window = _glfw.CreateWindow((int)_width, (int)_height, _title, null, null);

            _glfw.MakeContextCurrent(_window);
            _glfw.SwapInterval(2);

            Init();

            while (!_glfw.WindowShouldClose(_window))
            {
                Update(0);
                Render();
                _glfw.SwapBuffers(_window);
                _glfw.PollEvents();
            }

            Close();

            _glfw.Terminate();
        }
    }
}
