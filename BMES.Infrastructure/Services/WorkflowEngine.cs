using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq; // Added for OrderBy

namespace BMES.Infrastructure.Services
{
    public class WorkflowEngine : IHostedService, IWorkflowEngine
    {
        private readonly ILogger<WorkflowEngine> _logger;
        private readonly IOpcUaService _opcUaService;
        private readonly ITagConfigurationService _tagConfigurationService;
        private readonly IOrderRepository _orderRepository;

        private readonly ConcurrentDictionary<int, CancellationTokenSource> _activeWorkflows = new ConcurrentDictionary<int, CancellationTokenSource>();

        public WorkflowEngine(ILogger<WorkflowEngine> logger, IOpcUaService opcUaService, ITagConfigurationService tagConfigurationService, IOrderRepository orderRepository)
        {
            _logger = logger;
            _opcUaService = opcUaService;
            _tagConfigurationService = tagConfigurationService;
            _orderRepository = orderRepository;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Workflow Engine started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Workflow Engine stopping.");
            foreach (var cts in _activeWorkflows.Values)
            {
                cts.Cancel();
            }
            return Task.CompletedTask;
        }

        public async Task StartWorkflow(ProcessWorkflow workflow, ProductionOrder order)
        {
            if (workflow == null || order == null)
            {
                _logger.LogWarning("Attempted to start a workflow with null workflow or order.");
                return;
            }

            if (_activeWorkflows.ContainsKey(order.Id))
            {
                _logger.LogWarning($"Workflow already active for Production Order {order.OrderNumber}.");
                return;
            }

            var cts = new CancellationTokenSource();
            _activeWorkflows.TryAdd(order.Id, cts);

            _logger.LogInformation($"Starting workflow '{workflow.Name}' for Production Order '{order.OrderNumber}'.");

            try
            {
                order.Status = OrderStatus.InProgress;
                await _orderRepository.UpdateOrderAsync(order);

                foreach (var step in workflow.Steps.OrderBy(s => s.Order))
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        _logger.LogInformation($"Workflow for order {order.OrderNumber} cancelled.");
                        order.Status = OrderStatus.Cancelled;
                        await _orderRepository.UpdateOrderAsync(order);
                        break;
                    }

                    _logger.LogInformation($"Executing step {step.Order}: {step.Name} ({step.Description}) for order {order.OrderNumber}.");

                    switch (step)
                    {
                        case ControlStep controlStep:
                            string? controlNodeId = _tagConfigurationService.GetNodeId(controlStep.TagName!);
                            if (!string.IsNullOrEmpty(controlNodeId))
                            {
                                await _opcUaService.WriteTagAsync(controlNodeId, controlStep.Value!);
                                _logger.LogInformation($"ControlStep: Wrote {controlStep.Value} to {controlStep.TagName} ({controlNodeId}).");
                            }
                            else
                            {
                                _logger.LogError($"ControlStep: Tag '{controlStep.TagName}' not found in configuration.");
                            }
                            break;
                        case MonitorStep monitorStep:
                            _logger.LogInformation($"MonitorStep: Waiting for condition '{monitorStep.Condition}' on tag '{monitorStep.TagName}'. (Not implemented yet)");
                            await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
                            break;
                        case OperatorInstructionStep instructionStep:
                            _logger.LogInformation($"OperatorInstructionStep: Displaying instruction '{instructionStep.InstructionText}'. (Requires UI interaction - Not implemented yet)");
                            await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
                            break;
                        case DataCollectionStep dataCollectionStep:
                            _logger.LogInformation($"DataCollectionStep: Collecting data for '{dataCollectionStep.DataPointName}' from tag '{dataCollectionStep.TagName}'. (Not implemented yet)");
                            await Task.Delay(TimeSpan.FromSeconds(2), cts.Token);
                            break;
                        default:
                            _logger.LogWarning($"Unknown workflow step type: {step.GetType().Name}.");
                            break;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                if (!cts.Token.IsCancellationRequested)
                {
                    order.Status = OrderStatus.Completed;
                    await _orderRepository.UpdateOrderAsync(order);
                    _logger.LogInformation($"Workflow for order {order.OrderNumber} completed successfully.");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"Workflow for order {order.OrderNumber} was cancelled externally.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing workflow for order {order.OrderNumber}.");
                order.Status = OrderStatus.Cancelled;
                await _orderRepository.UpdateOrderAsync(order);
            }
            finally
            {
                _activeWorkflows.TryRemove(order.Id, out _);
            }
        }

        public void PauseWorkflow(ProductionOrder order)
        {
            _logger.LogInformation($"Attempted to pause workflow for order {order.OrderNumber}. (Not implemented)");
        }

        public void CancelWorkflow(ProductionOrder order)
        {
            if (_activeWorkflows.TryGetValue(order.Id, out var cts))
            {
                cts.Cancel();
                _logger.LogInformation($"Cancellation requested for workflow of order {order.OrderNumber}.");
            }
            else
            {
                _logger.LogWarning($"No active workflow found for Production Order {order.OrderNumber} to cancel.");
            }
        }
    }
}
