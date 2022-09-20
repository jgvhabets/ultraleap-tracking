"""
Importing Handtrack-Files from Ultraleap
"""

# Import public packages and fucntions
import os
import numpy as np
import pandas as pd

# Import own functions
##

def find_project_dir(cwd):
    """
    Finds upstream project folder which
    contains folders data, code, figures, etc.
    
    Input:
        - cwd (str): current working directory
            (os.getcwd())
    
    Returns:
        - project_dir (str): Path of project folder
    """
    for rep in range(10):

        if cwd[-4:] == 'code':
            project_dir = os.path.dirname(cwd)
            break
        
        else:
            cwd = os.path.dirname(cwd)
    
    return project_dir


def unityData_2_DataFrame(df):

    dfnew = pd.DataFrame(data=df['Time'].values, columns=['time'])

    for key in df.keys():
        firstrow = df[key][0]

        if key == 'Time': continue

        elif type(firstrow) == str:

            new_arr, new_names = convert_3ax_str_2_floats(
                values=df[key], col_name=key
            )
            dfnew[new_names] = new_arr

            
        else:
            dfnew[key] = df[key].values
    
    return dfnew


def convert_3ax_str_2_floats(
    values, col_name
):
    nrows = values.shape[0]
    new_arr = np.zeros((nrows, 3))
    for row in np.arange(nrows):
            rowstring = values[row][1:-1].split(', ')
            for c, str_val in enumerate(rowstring):
                new_arr[row, c] = float(str_val)

    new_names = [f'{col_name}_{ax}' for ax in ['x', 'y', 'z']]

    return new_arr, new_names


