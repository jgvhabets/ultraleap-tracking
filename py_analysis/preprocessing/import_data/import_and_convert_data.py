"""
Importing Handtrack-Files from Ultraleap
version data output 01.09.2022
"""

# Import public packages and fucntions
import numpy as np
import pandas as pd


def import_string_data(
    file_path: str,
):
    """
    Function to convert UltraLeap-data
    with incorrect commas and strings
    in to DataFrame

    Input:
        - file_path (str): directory and
            name of data file
    
    Returns:
        - df: pd DataFrame with correct data
    """


    # read in original data
    dat = np.loadtxt(file_path, dtype=str)

    # split keys to list
    keys = dat[0]
    keys = keys.split(',')

    # remove key row from data
    dat = dat[1:]

    list_of_values = []

    for row in np.arange(len(dat)):

        # create value list per row


        # split big string in pieces
        datsplit = dat[row].split(',')

        # take out single values global time and is pinching
        glob_time = datsplit[0]
        is_pinch = int(datsplit[-9])

        datsplit.pop(0)
        datsplit.pop(-9)

        # fill new list with floats
        values = []

        for i in np.arange(0, len(datsplit), 2):

            values.append(
                float(f'{datsplit[i]}.{datsplit[i + 1]}')
            )

        # insert single values in correct order to keys
        values.insert(0, glob_time)
        values.insert(-4, is_pinch)

        list_of_values.append(values)
    
    # convert list of lists to DataFrame
    df = pd.DataFrame(data=list_of_values, columns=keys)

    return df

