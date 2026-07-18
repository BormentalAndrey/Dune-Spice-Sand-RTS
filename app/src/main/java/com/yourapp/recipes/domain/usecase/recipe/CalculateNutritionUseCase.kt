package com.yourapp.recipes.domain.usecase.recipe

import com.yourapp.recipes.domain.model.NutritionInfo
import javax.inject.Inject

class CalculateNutritionUseCase @Inject constructor() {
    
    operator fun invoke(baseNutrition: NutritionInfo, multiplier: Float): NutritionInfo {
        return NutritionInfo(
            calories = baseNutrition.calories * multiplier,
            proteins = baseNutrition.proteins * multiplier,
            fats = baseNutrition.fats * multiplier,
            carbohydrates = baseNutrition.carbohydrates * multiplier
        )
    }
}
