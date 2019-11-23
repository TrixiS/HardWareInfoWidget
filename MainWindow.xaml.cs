using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using NvAPIWrapper.GPU;
using System.Windows.Input;

namespace InfoWidget
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private DateTime startTime;
		private PerformanceCounter cpuCounter;
		private PerformanceCounter ramCounter;
		private ulong totalMemory;

		public MainWindow()
		{
			InitializeComponent();
			startTime = DateTime.Now;
			NameBox.Text = Environment.UserName;

			totalMemory = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1000000;
			cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
			ramCounter = new PerformanceCounter("Memory", "Available MBytes");

			RamBar.Maximum = (double)totalMemory;
			CloseImage.MouseLeftButtonUp += CloseImage_LeftButtonClick;
		}

		private void CloseImage_LeftButtonClick(object sender, MouseButtonEventArgs e) => Close();
		private void MainWindow_Load(object sender, EventArgs e) => Update();

		private (int, int) GetGPU()
		{
			PhysicalGPU GPU = PhysicalGPU.GetPhysicalGPUs()[0];
			return (GPU.UsageInformation.GPU.Percentage, GPU.ThermalInformation.ThermalSensors.ToArray()[0].CurrentTemperature);
		}

		private async void Update()
		{
			double cpuUsage, ramUsage;	

			while (true)
			{
				cpuUsage = cpuCounter.NextValue();
				ramUsage = ramCounter.NextValue();

				UptimeBox.Dispatcher.Invoke(new Action(() =>
				{
					var uptime = DateTime.Now - startTime;
					UptimeBox.Text = $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
				}));
				
				CpuBar.Dispatcher.Invoke(new Action(() =>
				{
					CpuBar.Value = cpuUsage;
					CpuBarLabel.Content = $"{Math.Round(cpuUsage)}/100";
				}));

				RamBar.Dispatcher.Invoke(new Action(() =>
				{
					RamBar.Value = ((double)totalMemory) - ramUsage;
					RamBarLabel.Content = $"{((double)totalMemory) - Math.Round(ramUsage)}/{totalMemory}";
				}));

				GpuBar.Dispatcher.Invoke(new Action(() =>
				{
					var GPUInfo = GetGPU();
					GpuBox.Text = $"GPU - {GPUInfo.Item2} C";
					GpuBar.Value = GPUInfo.Item1;
					GpuBarLabel.Content = $"{GPUInfo.Item1}/100";
				}));

				await Task.Delay(1000);
			}
		}
	}
}
