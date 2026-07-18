package com.yourapp.recipes.domain.usecase.recipe

import com.yourapp.recipes.domain.repository.RecipeRepository
import javax.inject.Inject

class ToggleFavoriteUseCase @Inject constructor(
    private val recipeRepository: RecipeRepository
) {
    suspend operator fun invoke(recipeId: Long, isFavorite: Boolean) {
        recipeRepository.updateFavoriteStatus(recipeId, isFavorite)
    }
}
