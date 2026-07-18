package com.yourapp.recipes.domain.model

data class Recipe(
    val id: Long = 0,
    val title: String,
    val description: String = "",
    val cookingTimeMinutes: Int = 0,
    val difficulty: Difficulty = Difficulty.EASY,
    val category: Category = Category.DINNER,
    val ingredients: List<Ingredient> = emptyList(),
    val steps: List<CookingStep> = emptyList(),
    val nutritionInfo: NutritionInfo = NutritionInfo(),
    val servings: Int = 2,
    val photoPath: String? = null,
    val isFavorite: Boolean = false,
    val dateAdded: Long = System.currentTimeMillis(),
    val dateModified: Long = System.currentTimeMillis()
)
