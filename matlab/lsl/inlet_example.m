
%% instantiate the library
disp('Loading the library...');
lib = lsl_loadlib();

% resolve a stream...
disp('Waiting for data on input stream...');
result = {};
while isempty(result)
    result = lsl_resolve_byprop(lib,'type','hand_position'); end

% create a new inlet
disp('Opening an inlet...');
inlet = lsl_inlet(result{1});

disp('Now receiving data...');

% options
sample_amount_total = 100000;

% prepare a figure
h = animatedline;
x = 1:1:sample_amount_total;
y_lh = zeros(1, sample_amount_total);
y_rh = zeros(1, sample_amount_total);
ylim = ([0 100]);

for i = 1:sample_amount_total
    % get data from the inlet
    [vec,ts] = inlet.pull_sample();
    
    % plot
    y_lh(i) = vec(7);
    y_rh(i) = vec(7);
    
    addpoints(h, x(i), y_lh(i));
    drawnow limitrate;
    
    % display on command line
    %fprintf('%.2f\t',vec(7));
    %fprintf('%.5f\n',ts);
    
end