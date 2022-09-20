"""
Calculating distances of UltraLeap tracked
handpoints
"""

# Import public packages and fucntions
import os
import numpy as np
import pandas as pd

# Import own functions
##


def eucl_dist(df, point1, point2):
    """
    Defines Euclidean distances per sample-row
    of two points of interest.

    Input:
        - df (DataFrame): df containing all samples
        - point1 / point2 (str): column-name
            of point of interest
    
    Returns:
        - eucl_dist (array): np array containing
            values of Eucl distance for every
            sample row
    """
    coor1 = df[[
        f'{point1}_{ax}' for ax in ['x', 'y', 'z']
    ]].values
    coor2 = df[[
        f'{point2}_{ax}' for ax in ['x', 'y', 'z']
    ]].values

    coor_diff = coor1 - coor2

    eucl_dist = np.sqrt(
        np.sum(
            coor_diff ** 2, axis=1
        )
    )

    return eucl_dist