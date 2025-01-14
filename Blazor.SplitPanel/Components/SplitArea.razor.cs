﻿using Blazor.SplitPanel.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor.SplitPanel.Components
{
    public partial class SplitArea
    {
        private string cursor;
        private string gutterClass;
        private string _directionClass;

        public List<SplitPane> Panes { get; private set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public int GutterSize { get; set; }

        [Parameter]
        public SplitDirection Direction { get; set; }

        [Parameter]
        public string CssClass { get; set; }

        [Parameter]
        public string Cursor
        {
            get => cursor ?? (Direction == SplitDirection.Horizontal ? "col-resize" : "row-resize");
            set => cursor = value;
        }

        [Parameter]
        public string GutterClass
        {
            get => gutterClass ?? (Direction == SplitDirection.Horizontal ? "gutter gutter__horizontal" : "gutter gutter__vertical");
            set => gutterClass = value;
        }

        [Parameter]
        public string DirectionClass
        {
            get => _directionClass ?? (Direction == SplitDirection.Horizontal ? "split-area split-area__horizontal" : "split-area split-area__vertical");
            set => _directionClass = value;
        }

        [Parameter]
        public EventHandler<(SplitPane, SplitPane)> OnDragStart { get; set; }
        [Parameter]
        public EventHandler<(SplitPane, SplitPane)> OnDragEnd { get; set; }

        public void AddPane(SplitPane pane)
        {
            Panes.Add(pane);
            StateHasChanged();
        }

        public (SplitPane, SplitPane) GetPanePair(SplitPane pane)
        {
            return new(Panes[Panes.IndexOf(pane) - 1], pane);
        }

        public double GetPaneAutoSize()
        {
            var availibleSize = 100 - Panes.Sum(x => x.Size ?? 0);
            var panesWithoutInitialSize = Panes.Where(x => !x.Size.HasValue);

            return availibleSize / panesWithoutInitialSize.Count();
        }

        public async Task DragStartAsync((SplitPane, SplitPane) pair)
        {
            List<Task> tasks = new();
            foreach (var pane in Panes)
            {
                var task = pane.JsInterop.SetElementStyleAsync(pane.PaneElement, "userSelect", "none");
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        public async Task DragEndAsync((SplitPane, SplitPane) pair)
        {
            List<Task> tasks = new List<Task>();
            foreach (var pane in Panes)
            {
                var task = pane.JsInterop.SetElementStyleAsync(pane.PaneElement, "userSelect", "");
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }



        protected override void OnInitialized()
        {
            base.OnInitialized();
            Panes = new List<SplitPane>();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                foreach (var pane in Panes.Where(x => !x.Size.HasValue))
                {
                    await pane.SetSizeAsync(GetPaneAutoSize(), false);
                }
            }
        }
    }
}
